using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp;

namespace BlockM3.AEternity.SwaggerClientGenerator
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException("You must provide: sourceapidir destinationdir");
            string sourcepath = args[0];
            string destinationpath = args[1];
            string[] sources = Directory.GetFiles(sourcepath, "s*.yml");
            if (sources.Length != 2)
                throw new ArgumentException("Expecting only 2 swagger definitions in api directory (api and compiler)");

            CSharpClientGeneratorSettings apisettings = new CSharpClientGeneratorSettings {CSharpGeneratorSettings = {Namespace = "BlockM3.AEternity.SDK.Generated.Api", SchemaType = SchemaType.OpenApi3, PropertyNameGenerator = new CustomTypeScriptPropertyNameGenerator()}, AdditionalNamespaceUsages = new[] {"BlockM3.AEternity.SDK.Generated.Models", "System.Numerics"}, AdditionalContractNamespaceUsages = new[] {"System.Numerics"}};
            CSharpClientGeneratorSettings compilersettings = new CSharpClientGeneratorSettings
            {
                CSharpGeneratorSettings = {ExcludedTypeNames = new[] {"ByteCode", "Error"}, Namespace = "BlockM3.AEternity.SDK.Generated.Compiler", SchemaType = SchemaType.OpenApi3, PropertyNameGenerator = new CustomTypeScriptPropertyNameGenerator()}, AdditionalNamespaceUsages = new[] {"BlockM3.AEternity.SDK.Generated.Models", "System.Numerics"}, AdditionalContractNamespaceUsages = new[] {"System.Numerics"}, GenerateExceptionClasses = false,
            };
            string compiler = sources.FirstOrDefault(a => a.Contains("compiler"));
            if (compiler == null)
                throw new ArgumentException("Expecting compiler api definition in api");
            string api = sources.First(a => a != compiler);
            OpenApiDocument apidocument = await OpenApiYamlDocument.FromFileAsync(api);
            OpenApiDocument compilerdocument = await OpenApiYamlDocument.FromFileAsync(compiler);
            var apigenerator = new CSharpClientGenerator(apidocument, apisettings, CreateResolverWithExceptionSchema(apisettings.CSharpGeneratorSettings, apidocument));
            var compilergenerator = new CSharpClientGenerator(compilerdocument, compilersettings, CreateResolverWithExceptionSchema(compilersettings.CSharpGeneratorSettings, compilerdocument));
            string apiclient = apigenerator.GenerateFile(ClientGeneratorOutputType.Implementation);
            apigenerator.Settings.CSharpGeneratorSettings.Namespace = "BlockM3.AEternity.SDK.Generated.Models";
            string apimodels = apigenerator.GenerateFile(ClientGeneratorOutputType.Contracts);
            string compilerclient = compilergenerator.GenerateFile(ClientGeneratorOutputType.Implementation);
            compilergenerator.Settings.CSharpGeneratorSettings.Namespace = "BlockM3.AEternity.SDK.Generated.Models";
            string compilermodels = compilergenerator.GenerateFile(ClientGeneratorOutputType.Contracts);
            string apipath = Path.Combine(destinationpath, "Api");
            string compilerpath = Path.Combine(destinationpath, "Compiler");
            string modelpath = Path.Combine(destinationpath, "Models");
            try
            {
                Directory.CreateDirectory(apipath);
            }
            catch
            {
                // ignored
            }

            try
            {
                Directory.CreateDirectory(compilerpath);
            }
            catch
            {
                //ignored
            }

            try
            {
                Directory.CreateDirectory(modelpath);
            }
            catch
            {
                //ignored
            }

            await File.WriteAllTextAsync(Path.Combine(apipath, "Client.cs"), apiclient, Encoding.UTF8);
            await File.WriteAllTextAsync(Path.Combine(compilerpath, "Client.cs"), compilerclient, Encoding.UTF8);
            await File.WriteAllTextAsync(Path.Combine(modelpath, "Models.cs"), apimodels, Encoding.UTF8);
            await File.WriteAllTextAsync(Path.Combine(modelpath, "ModelsCompiler.cs"), compilermodels, Encoding.UTF8);
        }

        public static CSharpTypeResolver CreateResolverWithExceptionSchema(CSharpGeneratorSettings settings, OpenApiDocument document)
        {
            var exceptionSchema = document.Definitions.ContainsKey("Exception") ? document.Definitions["Exception"] : null;

            var resolver = new CustomResolver(settings, exceptionSchema);
            resolver.RegisterSchemaDefinitions(document.Definitions.Where(p => p.Value != exceptionSchema).ToDictionary(p => p.Key, p => p.Value));

            return resolver;
        }
    }

    public class CustomTypeScriptPropertyNameGenerator : IPropertyNameGenerator
    {
        public string Generate(JsonSchemaProperty property)
        {
            string f = string.Join("", property.Name.Split(new char[] {'_', '-'}).ToList().Select((a) => a.First().ToString().ToUpperInvariant() + a.Substring(1)));
            //C# do not support property names with the same name of the parent class, change nswag behavior with this replace
            return f.Replace("Calldata", "CallData").Replace("Tx", "TX").Replace("CommitmentId", "CommitmentID").Replace("PubKey", "PublicKey").Replace("Peers", "PeersCollection").Replace("OracleQueries", "OracleQueriesCollection");
        }
    }

    public class CustomResolver : CSharpTypeResolver
    {
        //This will handle correct type serialization for integers
        public CustomResolver(CSharpGeneratorSettings settings) : base(settings)
        {
        }

        public CustomResolver(CSharpGeneratorSettings settings, JsonSchema exceptionSchema) : base(settings, exceptionSchema)
        {
        }

        public override string Resolve(JsonSchema schema, bool isNullable, string typeNameHint)
        {
            //Added support for uint64
            var type = schema.ActualTypeSchema.Type;
            if (type.HasFlag(JsonObjectType.Integer))
            {
                var scj = schema.ActualTypeSchema;
                if (scj.Format == "uint64")
                    return isNullable ? "ulong?" : "ulong";
                if (scj.Format == "int64")
                    return isNullable ? "long?" : "long";
                if (scj.Maximum.HasValue)
                {
                    if (scj.Maximum.Value <= 65535)
                    {
                        if (scj.Minimum.HasValue && scj.Minimum.Value < 0)
                            return isNullable ? "short?" : "short";
                        return isNullable ? "ushort?" : "ushort";
                    }

                    if (scj.Maximum.Value <= 4294967295)
                    {
                        if (scj.Minimum.HasValue && scj.Minimum.Value < 0)
                            return isNullable ? "int?" : "int";
                        return isNullable ? "uint?" : "uint";
                    }

                    if (scj.Maximum.Value <= 18446744073709551615)
                    {
                        if (scj.Minimum.HasValue && scj.Minimum.Value < 0)
                            return isNullable ? "long?" : "long";
                        return isNullable ? "ulong?" : "ulong";
                    }
                }
                else
                {
                    return isNullable ? "BigInteger?" : "BigInteger";
                }
            }

            return base.Resolve(schema, isNullable, typeNameHint);
        }
    }
}