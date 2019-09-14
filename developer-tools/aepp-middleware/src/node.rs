use curl::easy::{Easy, List};
use regex::Regex;
use serde_json;
use serde_json::Value;
use std;
use std::collections::HashMap;
use std::io::Read;
use std::str;

use loader::SQLCONNECTION;
use middleware_result::MiddlewareError;
use middleware_result::MiddlewareResult;
use models::InsertableMicroBlock;
use models::JsonKeyBlock;
use models::JsonTransaction;

#[derive(Debug)]
pub struct HttpResponse {
    pub status: Option<String>,
    pub message: Option<String>,
    pub headers: HashMap<String, String>,
    pub body: Option<String>,
}

impl HttpResponse {
    pub fn new() -> Self {
        HttpResponse {
            status: None,
            message: None,
            headers: HashMap::new(),
            body: None,
        }
    }

    pub fn store_http_headers(&mut self, header: &String) -> bool {
        if let Some((status, message)) = http_header(&header) {
            self.status = Some(status);
            self.message = Some(message);
        }
        let hdr: Vec<&str> = header.splitn(2, ": ").collect();
        if let Some(value) = hdr.get(1) {
            self.headers
                .insert(hdr.get(0).unwrap().to_string(), value.to_string());
        }
        true
    }
}

pub struct Node {
    base_uri: String,
}

impl Node {
    pub fn new(base_url: String) -> Node {
        Node { base_uri: base_url }
    }

    pub fn clone(&self) -> Node {
        Node::new(self.base_uri.clone())
    }

    pub fn get_missing_heights(&self, height: i64) -> MiddlewareResult<Vec<i32>> {
        let sql = format!("SELECT * FROM generate_series(0,{}) s(i) WHERE NOT EXISTS (SELECT height FROM key_blocks WHERE height = s.i)", height);
        debug!("{}", &sql);
        let mut missing_heights = Vec::new();
        for row in SQLCONNECTION.get()?.query(&sql, &[])?.iter() {
            missing_heights.push(row.get(0));
        }
        Ok(missing_heights)
    }

    pub fn current_generation(&self) -> MiddlewareResult<serde_json::Value> {
        self.get(&String::from("generations/current"))
    }

    pub fn get_generation_at_height(&self, height: i64) -> MiddlewareResult<serde_json::Value> {
        let path = format!("generations/height/{}", height);
        self.get(&String::from(path))
    }

    pub fn latest_key_block(&self) -> MiddlewareResult<serde_json::Value> {
        self.get(&String::from("key-blocks/current"))
    }

    pub fn transaction_info(
        &self,
        transaction_hash: &String,
    ) -> MiddlewareResult<serde_json::Value> {
        self.get(&format!("transactions/{}/info", transaction_hash))
    }

    pub fn get(&self, operation: &String) -> MiddlewareResult<serde_json::Value> {
        debug!("Fetching {}", operation);
        let http_response = self.get_naked(&String::from("/v2/"), operation)?;
        match http_response.body {
            Some(body) => Ok(serde_json::from_str(&body)?),
            None => Err(MiddlewareError::new(&format!(
                "GET failed, response was {:?}",
                http_response
            ))),
        }
    }

    // Get a URL, and return the response
    pub fn get_naked(&self, prefix: &String, operation: &String) -> MiddlewareResult<HttpResponse> {
        let uri = self.base_uri.clone() + prefix + operation;
        debug!("Fetching {}", uri);
        let mut http_response = HttpResponse::new();
        let mut response = Vec::new();
        let mut handle = Easy::new();
        handle.timeout(std::time::Duration::from_secs(20))?;
        handle.url(&uri)?;
        {
            let mut transfer = handle.transfer();
            transfer.write_function(|new_data| {
                response.extend_from_slice(new_data);
                Ok(new_data.len())
            })?;
            transfer.header_function(|header| {
                let hdr_str = String::from_utf8(header.to_vec()).unwrap();
                http_response.store_http_headers(&hdr_str)
            })?;
            transfer.perform()?;
        }
        http_response.body = Some(String::from(std::str::from_utf8(&response)?));
        Ok(http_response)
    }

    pub fn post_naked(
        &self,
        prefix: &String,
        operation: &String,
        body: String,
    ) -> MiddlewareResult<(HashMap<String, String>, String)> {
        let uri = self.base_uri.clone() + prefix + operation;
        let mut data = body.as_bytes();
        let mut handle = Easy::new();
        handle.url(&uri)?;
        let mut list = List::new();
        list.append("content-type: application/json")?;
        handle.http_headers(list)?;
        handle.post(true)?;
        handle.post_field_size(data.len() as u64)?;
        let mut response = Vec::new();
        let mut http_response = HttpResponse::new();
        {
            let mut transfer = handle.transfer();
            transfer.read_function(|buf| Ok(data.read(buf).unwrap_or(0)))?;
            transfer.header_function(|header| {
                let hdr_str = String::from_utf8(header.to_vec()).unwrap();
                http_response.store_http_headers(&hdr_str)
            })?;
            transfer.write_function(|new_data| {
                response.extend_from_slice(new_data);
                Ok(new_data.len())
            })?;
            transfer.perform()?;
        }
        let resp = String::from(std::str::from_utf8(&response)?);
        Ok((http_response.headers, resp))
    }

    pub fn get_key_block_by_hash(&self, hash: &String) -> MiddlewareResult<serde_json::Value> {
        let result = self.get(&format!("{}{}", String::from("key-blocks/hash/"), &hash))?;
        Ok(result)
    }

    pub fn get_key_block_by_height(&self, height: i64) -> MiddlewareResult<serde_json::Value> {
        let result = self.get(&format!(
            "{}{}",
            String::from("key-blocks/height/"),
            &height
        ))?;
        Ok(result)
    }

    pub fn get_micro_block_by_hash(&self, hash: &String) -> MiddlewareResult<serde_json::Value> {
        let result = self.get(&format!(
            "{}{}{}",
            String::from("micro-blocks/hash/"),
            &hash,
            String::from("/header")
        ))?;
        Ok(result)
    }

    pub fn get_transaction_list_by_micro_block(
        &self,
        hash: &String,
    ) -> MiddlewareResult<serde_json::Value> {
        let result = self.get(&format!(
            "{}{}{}",
            String::from("micro-blocks/hash/"),
            &hash,
            String::from("/transactions")
        ))?;
        Ok(result)
    }

    pub fn get_pending_transaction_list(&self) -> MiddlewareResult<serde_json::Value> {
        let result = self.get(&String::from("debug/transactions/pending"))?;
        Ok(result)
    }
}

pub fn from_json(val: &String) -> String {
    let re = Regex::new("^\"(.*)\"$").unwrap();
    match re.captures(val) {
        Some(matches) => String::from(&matches[1]),
        None => val.clone(),
    }
}

pub fn key_block_from_json(json: Value) -> MiddlewareResult<JsonKeyBlock> {
    let block: JsonKeyBlock = serde_json::from_value(json)?;
    Ok(block)
}

pub fn micro_block_from_json(json: Value) -> MiddlewareResult<InsertableMicroBlock> {
    let block: InsertableMicroBlock = serde_json::from_value(json)?;
    Ok(block)
}

pub fn transaction_from_json(json: Value) -> MiddlewareResult<JsonTransaction> {
    let transaction: JsonTransaction = serde_json::from_value(json)?;
    Ok(transaction)
}

fn http_header(header: &String) -> Option<(String, String)> {
    lazy_static! {
        static ref STATUS_REGEX: Regex = Regex::new(r"HTTP/1.[01]\s+([0-9]{3})\s+(.+)").unwrap();
    }
    if let Some(captures) = STATUS_REGEX.captures(&header) {
        return Some((
            String::from(captures.get(1)?.as_str()),
            String::from(captures.get(2)?.as_str()),
        ));
    }
    None
}

#[test]
fn test_http_header() {
    let (status, message) = http_header(&String::from("HTTP/1.1 200 AOK")).unwrap();
    assert_eq!(status, "200");
    assert_eq!(message, "AOK");
    let (status, message) = http_header(&String::from("HTTP/1.0 404 Not found")).unwrap();
    assert_eq!(status, "404");
    assert_eq!(message, "Not found");
    let (status, message) = http_header(&String::from("HTTP/1.0 301 Not modified")).unwrap();
    assert_eq!(status, "301");
    assert_eq!(message, "Not modified");
}
