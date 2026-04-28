FROM alpine:3.23
ARG GOOSE_VERSION=v3.27.0
WORKDIR /app
ADD https://github.com/pressly/goose/releases/download/${GOOSE_VERSION}/goose_linux_x86_64 /app/goose
RUN chmod +x goose
COPY MarketBasketAnalysisStorage.Data/Migrations .
RUN adduser -D app
USER app
ENTRYPOINT /app/goose postgres "$CONNECTION_STRING" up
EXPOSE 8080