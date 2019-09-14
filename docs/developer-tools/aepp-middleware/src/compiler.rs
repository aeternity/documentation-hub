use reqwest::header::{HeaderMap, HeaderName, HeaderValue};
use reqwest::StatusCode;

use middleware_result::MiddlewareResult;

pub fn validate_compiler(compiler_version: String) -> bool {
    match supported_compiler_versions() {
        Ok(result) => match result {
            Some(compilers) => match compilers.iter().find(|val| val == &&compiler_version) {
                Some(_x) => true,
                _ => false,
            },
            _ => false,
        },
        _ => false,
    }
}

/**
 * Compile the contract and return the byte code
 */

pub fn compile_contract(source: String, compiler: String) -> MiddlewareResult<Option<String>> {
    let compiler_host = std::env::var("AESOPHIA_URL")?;
    let client = reqwest::Client::new();
    let mut headers = HeaderMap::new();
    headers.insert(
        HeaderName::from_static("sophia-compiler-version"),
        HeaderValue::from_str(&compiler)?,
    );
    debug!("Compiler Headers {:?}", headers);
    let options: serde_json::Value = serde_json::from_str("{}")?;
    let result: serde_json::Value = client
        .post(&format!("{}/compile", compiler_host))
        .headers(headers)
        .json(&json!({
            "code": source,
            "options": options
        }))
        .send()?
        .json()?;
    debug!("Compiler Result {:?}", result);
    match result["bytecode"].as_str() {
        Some(bytecode) => Ok(Some(bytecode.to_string())),
        _ => Ok(None),
    }
}

/**
 * Check attached compiler version and return a list of versions it supports
 * 1. Check if the compiler is behind a proxy that supports multiple compiler and compiler switching
 * 2. Else return the `/version` in an array so that result is consistent
 */

pub fn supported_compiler_versions() -> MiddlewareResult<Option<Vec<String>>> {
    if let Ok(contract_url) = std::env::var("AESOPHIA_URL") {
        let client = reqwest::Client::new();
        let mut headers = HeaderMap::new();
        headers.insert(
            HeaderName::from_static("accept"),
            HeaderValue::from_static("application/json"),
        );
        let mut res = client.get(&contract_url).headers(headers).send()?;
        return match res.status() {
            StatusCode::OK => {
                let data: String = res.text()?;
                let response: serde_json::Value = serde_json::from_str(&data)?;
                debug!("{:?}", response);
                match response["Compilers"].as_array() {
                    Some(data) => {
                        debug!("{:?}", data);
                        let result: Vec<String> = data
                            .iter()
                            .map(|val: &serde_json::Value| {
                                String::from(val["version"].as_str().unwrap())
                            })
                            .collect();
                        Ok(Some(result))
                    }
                    _ => Ok(None),
                }
            }
            StatusCode::NOT_FOUND => {
                match client.get(&format!("{}/version", contract_url)).send() {
                    Ok(mut data) => {
                        debug!("{:?}", data);
                        let response: serde_json::Value = serde_json::from_str(&data.text()?)?;
                        Ok(Some(vec![String::from(response["version"].as_str()?)]))
                    }
                    _ => Ok(None),
                }
            }
            _ => Ok(None),
        };
    }
    Ok(None)
}
