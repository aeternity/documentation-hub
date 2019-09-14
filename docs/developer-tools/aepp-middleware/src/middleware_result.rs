use serde_json;
use std::error::Error;
use std::fmt;
use std::option::NoneError;
use ws::Error as WsError;

#[derive(Debug)]
pub struct MiddlewareError {
    details: String,
}

impl MiddlewareError {
    pub fn new(msg: &str) -> MiddlewareError {
        let bt = backtrace::Backtrace::new();

        MiddlewareError {
            details: format!("{}\n{:?}", msg.to_string(), bt),
        }
    }
}

impl fmt::Display for MiddlewareError {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        write!(f, "{}", self.details)
    }
}

impl std::error::Error for MiddlewareError {
    fn description(&self) -> &str {
        &self.details
    }
    fn cause(&self) -> Option<&dyn Error> {
        // Generic error, underlying cause isn't tracked.
        None
    }
}

impl From<NoneError> for MiddlewareError {
    fn from(_none: std::option::NoneError) -> Self {
        MiddlewareError::new("None")
    }
}

impl From<r2d2::Error> for MiddlewareError {
    fn from(err: r2d2::Error) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl From<Box<dyn std::error::Error>> for MiddlewareError {
    fn from(err: Box<dyn std::error::Error>) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<serde_json::Error> for MiddlewareError {
    fn from(err: serde_json::Error) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<diesel::result::Error> for MiddlewareError {
    fn from(err: diesel::result::Error) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<postgres::Error> for MiddlewareError {
    fn from(err: postgres::Error) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<curl::Error> for MiddlewareError {
    fn from(err: curl::Error) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<std::str::Utf8Error> for MiddlewareError {
    fn from(err: std::str::Utf8Error) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<std::sync::mpsc::TryRecvError> for MiddlewareError {
    fn from(err: std::sync::mpsc::TryRecvError) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<std::sync::mpsc::SendError<i64>> for MiddlewareError {
    fn from(err: std::sync::mpsc::SendError<i64>) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<WsError> for MiddlewareError {
    fn from(err: WsError) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<std::string::FromUtf8Error> for MiddlewareError {
    fn from(err: std::string::FromUtf8Error) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<curl::FormError> for MiddlewareError {
    fn from(err: curl::FormError) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<reqwest::Error> for MiddlewareError {
    fn from(err: reqwest::Error) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl
    std::convert::From<
        std::sync::PoisonError<
            std::sync::MutexGuard<'_, std::cell::RefCell<std::vec::Vec<crate::websocket::Client>>>,
        >,
    > for MiddlewareError
{
    fn from(
        err: std::sync::PoisonError<
            std::sync::MutexGuard<'_, std::cell::RefCell<std::vec::Vec<crate::websocket::Client>>>,
        >,
    ) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<bigdecimal::ParseBigDecimalError> for MiddlewareError {
    fn from(err: bigdecimal::ParseBigDecimalError) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<std::env::VarError> for MiddlewareError {
    fn from(err: std::env::VarError) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<reqwest::header::InvalidHeaderValue> for MiddlewareError {
    fn from(err: reqwest::header::InvalidHeaderValue) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}

impl std::convert::From<base64::DecodeError> for MiddlewareError {
    fn from(err: base64::DecodeError) -> Self {
        MiddlewareError::new(&err.to_string())
    }
}
pub type MiddlewareResult<T> = Result<T, MiddlewareError>;
