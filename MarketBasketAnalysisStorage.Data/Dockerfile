FROM alpine:3.23
RUN apk add --no-cache postgresql-client
WORKDIR /app
COPY MarketBasketAnalysisStorage.Data/migrations.sql .
ENTRYPOINT psql -d "$CONNECTION_STRING" -v ON_ERROR_STOP=1 -f /app/migrations.sql