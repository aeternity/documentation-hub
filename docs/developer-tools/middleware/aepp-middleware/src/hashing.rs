use base58::ToBase58;
pub use base58check::FromBase58Check;
use blake2::digest::{Input, Reset, VariableOutput};
use blake2::VarBlake2b;
pub use blake2b::Blake2b;
pub use byteorder::{BigEndian, WriteBytesExt};
use crypto::digest::Digest;
use crypto::sha2::Sha256;

use middleware_result::MiddlewareResult;

/*
 * taken from https://github.com/dotcypress/base58check
 * reproduced with kind permission of the author
 */
fn double_sha256(payload: &[u8]) -> Vec<u8> {
    let mut hasher = Sha256::new();
    let mut hash = vec![0; hasher.output_bytes()];
    hasher.input(&payload);
    hasher.result(&mut hash);
    hasher.reset();
    hasher.input(&hash);
    hasher.result(&mut hash);
    hash.to_vec()
}

/* taken from https://github.com/dotcypress/base58check
 * reproduced with kind permission of the author
 */
fn to_base58check(data: &[u8]) -> String {
    let mut payload = data.to_vec();
    let mut checksum = double_sha256(&payload);
    payload.append(&mut checksum[..4].to_vec());
    payload.to_base58()
}

/*
 * get rid of the xx_ part from an æternity address returning the hash
*/
fn hash_part(id: &String) -> String {
    id[3..].to_string()
}

/*
 * generate a contract id by:
 * - decoding sender address into a byte array
 * - appending the nonce of the TX, as big-endian number with leading zero bytes removes
 * - taking the Blake2b 256 32-bit long hash of this
 * - re-encoding using base48check (æternity's modified version with no version byte)
 * - preprending ct_
 */
pub fn gen_contract_id(owner_id: &String, nonce: i64) -> String {
    let mut v = decodebase58check(&hash_part(owner_id));
    v.append(&mut min_b(nonce));
    let mut hasher = VarBlake2b::new(32).unwrap();
    hasher.input(v);
    let hash = hasher.vec_result();
    let encoded = to_base58check(&hash);
    format!("ct_{}", encoded)
}

fn blake2bdigest(v: &Vec<u8>) -> Vec<u8> {
    let mut hasher = VarBlake2b::new(32).unwrap();
    hasher.input(v);
    let hash = hasher.vec_result();
    hash
}

pub fn gen_oracle_query_id(sender_id: &String, nonce: i64, recipient_id: &String) -> String {
    let mut sender_id_bin = decodebase58check(&hash_part(sender_id));
    let mut recipient_id_bin = decodebase58check(&hash_part(recipient_id));
    let mut nonce_byte32 = min_b(nonce);
    loop {
        if nonce_byte32.len() < 32 {
            nonce_byte32.insert(0, 0u8);
        } else {
            break;
        }
    }
    let mut all = vec![];
    all.append(&mut sender_id_bin);
    all.append(&mut nonce_byte32);
    all.append(&mut recipient_id_bin);
    let hash = blake2bdigest(&all);
    let encoded = to_base58check(&hash);
    format!("oq_{}", encoded)
}

pub fn gen_channel_id(
    initiator_id: &String,
    channel_create_tx_nonce: i64,
    responder_id: &String,
) -> String {
    let mut initiator_id_bin = decodebase58check(&hash_part(initiator_id));
    let mut responder_id_bin = decodebase58check(&hash_part(responder_id));
    let mut nonce_byte32 = min_b(channel_create_tx_nonce);
    loop {
        if nonce_byte32.len() < 32 {
            nonce_byte32.insert(0, 0u8);
        } else {
            break;
        }
    }
    let mut all = vec![];
    all.append(&mut initiator_id_bin);
    all.append(&mut nonce_byte32);
    all.append(&mut responder_id_bin);
    let hash = blake2bdigest(&all);
    let encoded = to_base58check(&hash);
    format!("ch_{}", encoded)
}

pub fn get_name_hash(name: &str) -> Vec<u8> {
    let mut result = [0u8; 32].to_vec();
    let mut split: Vec<&[u8]> = name.split('.').rev().map(|s| s.as_bytes()).collect();
    loop {
        if let Some(part) = split.pop() {
            println!("{}", String::from_utf8(part.to_vec()).unwrap());
            let mut hasher = VarBlake2b::new(32).unwrap();
            hasher.input(part);
            let hashed = hasher.vec_result();
            result.extend(hashed);
            let mut hasher = VarBlake2b::new(32).unwrap();
            hasher.input(result);
            result = hasher.vec_result();
        } else {
            break;
        }
    }
    result
}

pub fn get_name_id(name: &str) -> MiddlewareResult<String> {
    Ok(format!("nm_{}", to_base58check(&get_name_hash(name))))
}

#[test]
fn test_name_hash() {
    assert_eq!(
        get_name_id("welghmolql.test").unwrap(),
        "nm_Ziiq3M9ASEHXCV71qUNde6SsomqwZjYPFvnJSvTkpSUDiXqH3"
    );
    assert_ne!(
        get_name_id("abc.test").unwrap(),
        "nm_2KrC4asc6fdv82uhXDwfiqB1TY2htjhnzwzJJKLxidyMymJRUQ"
    );
}

/*
 * decode base 58, adding the version byte onto the returned value
 */
fn decodebase58check(data: &String) -> Vec<u8> {
    let mut result = base58check::FromBase58Check::from_base58check(data.as_str())
        .unwrap()
        .1;
    result.insert(
        0,
        base58check::FromBase58Check::from_base58check(data.as_str())
            .unwrap()
            .0,
    );
    result
}

/*
 * take a number, encode it big-endian and then remove all leading zeros
*/
fn min_b(val: i64) -> Vec<u8> {
    let mut val = val;
    if val == 0 {
        val = 1;
    }
    let mut wtr = vec![];
    wtr.write_i64::<BigEndian>(val); // TODO: use this Result.
    let mut result: Vec<u8> = vec![];
    let mut zeros_gone = false;
    for byte in wtr {
        if zeros_gone || byte != 0 {
            zeros_gone = true;
            result.push(byte);
        }
    }
    result
}

#[test]
fn test_base58check() {
    let key = String::from("P1hn3JnJXcdx8USijBcgZHLgvZywH5PbjQK5G1iZaEu9obHiH");
    assert_eq!(
        decodebase58check(&key),
        [
            49, 251, 48, 224, 136, 182, 49, 247, 149, 81, 107, 110, 171, 235, 247, 240, 41, 195,
            59, 34, 112, 140, 205, 205, 96, 12, 98, 243, 187, 230, 36, 51
        ]
    );
    assert_eq!(to_base58check(&decodebase58check(&key)), key);
}

#[test]
fn test_gen_contract_id() {
    let address = String::from("ak_P1hn3JnJXcdx8USijBcgZHLgvZywH5PbjQK5G1iZaEu9obHiH");
    assert_eq!(
        gen_contract_id(&address, 2),
        String::from("ct_5ye5dEQwtCrRhsKYq8BprAMFptpY59THUyTxSBQKpDTcywEhk")
    );
}

#[test]
fn test_gen_oracle_query_id() {
    let sender_address = String::from("ak_2ZjpYpJbzq8xbzjgPuEpdq9ahZE7iJRcAYC1weq3xdrNbzRiP4");
    let nonce = 1;
    let oracle_address = String::from("ok_2iqfJjbhGgJFRezjX6Q6DrvokkTM5niGEHBEJZ7uAG5fSGJAw1");
    let expected_result = String::from("oq_2YvZnoohcSvbQCsPKSMxc98i5HZ1sU5mR6xwJUZC3SvkuSynMj");
    assert_eq!(
        gen_oracle_query_id(&sender_address, nonce, &oracle_address),
        expected_result
    );
}
