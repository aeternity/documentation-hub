using System;
using System.Collections.Generic;

namespace BlockM3.AEternity.SDK.Utils
{
    public static class Validation
    {
        public static int DOMAIN_NAME_MAX_LENGTH = 253;

        public static string LIST_NOT_SAME_SIZE = "Lists don't have the same size";

        public static string NO_ENTRIES = "List or map has no entries";

        public static string MAP_MISSING_VALUE = "Map is missing value for {0}";

        public static string NAMESPACE_INVALID = "Namespace not allowed / not provided.";

        public static string PARAMETER_IS_NULL = "Parameter cannot be null";

        public static string MISSING_API_IDENTIFIER = "Parameter does not start with APIIdentifier {0}_";

        public static string NAMESPACE_EXCEEDS_LIMIT = $"Domainname exceeds {DOMAIN_NAME_MAX_LENGTH} char limit.";

        public static List<string> ALLOWED_NAMESPACES = new List<string>() {"test"};

        public static string MissingApiIdentifier(string id) => string.Format(MISSING_API_IDENTIFIER, id);

        /**
         * encapsule validation of given parameters
         *
         * @param validationMethod the validation method to apply on the object, which should return an
         *     optional of boolean
         * @param objectToValidate the object to validate
         * @param methodName the method, where the validation takes places
         * @param parameters the parameter(s) which are validated
         * @param cause optional message for detailled explanation of the validation error
         */


        /* Yeah right
      
        public static void CheckParameters(Func<object, bool?> validationMethod, object objectToValidate, string methodName, List<string> parameters, params object[] cause)
        {
            bool? ret = validationMethod(objectToValidate);
            if ()
          if (validationMethod.apply(objectToValidate).orElseGet(() -> Boolean.FALSE).booleanValue()
              == false) {
            String causeMessage = "";
            if (cause != null) {
              if (cause.length > 1) {
                Object[] paramArray = Arrays.copyOfRange(cause, 1, cause.length);
                causeMessage = ": " + String.format(cause[0].toString(), paramArray);
              } else {
                causeMessage = ": " + cause[0].toString();
              }
            }
            throw new InvalidParameterException(
                String.format(
                    "Call of function %s has missing or invalid parameters %s%s",
                    methodName, parameters, causeMessage));
          }
        }
              */
        /**
         * validate the given domainName
         *
         * @param domainName
         */
        public static void CheckNamespace(string domainName)
        {
            string[] domainSplit = domainName.Split('.');

            if (domainSplit.Length != 2 || !ALLOWED_NAMESPACES.Contains(domainSplit[1]))
                throw new ArgumentException($"Invalid Parameter domainName: {NAMESPACE_INVALID}");
            if (domainName.Length > DOMAIN_NAME_MAX_LENGTH)
                throw new ArgumentException($"Invalid Parameter domainName: {NAMESPACE_EXCEEDS_LIMIT}");
        }
    }
}