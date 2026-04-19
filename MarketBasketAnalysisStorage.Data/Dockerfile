FROM alpine:3.23
WORKDIR /app
ADD https://github.com/pressly/goose/releases/download/v3.27.0/goose_linux_x86_64 /app/goose
RUN chmod +x goose
COPY MarketBasketAnalysisStorage.Data/Migrations .
ENTRYPOINT /app/goose postgres "$CONNECTION_STRING" up