FROM rust:1.35.0 as build

# maybe we can optimize this but easiest way to get nightly
RUN rustup default nightly
RUN USER=root cargo new --bin --vcs none aepp-middleware
WORKDIR /aepp-middleware

COPY ./Cargo.lock ./Cargo.lock
COPY ./Cargo.toml ./Cargo.toml
RUN touch ./src/lib.rs

RUN cargo build --release
RUN rm src/*.rs && rm -rf target/release/deps/mdw*
COPY ./src/ ./src/
COPY ./migrations/ ./migrations/
COPY ./swagger/ ./swagger/
RUN cargo build --release

FROM debian:stretch-slim
RUN apt-get update && apt-get -y install libpq5 libcurl3 libcurl3-gnutls
COPY --from=build /aepp-middleware/target/release/mdw /app/aepp-middleware
WORKDIR /app
COPY ./docker/Rocket.toml ./Rocket.toml
ENTRYPOINT ["/app/aepp-middleware"]
CMD ["-p", "-s", "-w"]
EXPOSE 80 3020
