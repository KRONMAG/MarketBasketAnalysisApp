-- +goose Up
CREATE TABLE association_rule_sets (
    id SERIAL,
    name TEXT NOT NULL,
    description TEXT,
    transactions_count BIGINT NOT NULL,
    CONSTRAINT pk_association_rule_sets PRIMARY KEY (id)
);

CREATE TABLE association_rule_chunks (
    id SERIAL,
    data SMALLINT[] NOT NULL,
    association_rule_set_id INT NOT NULL,
    CONSTRAINT pk_association_rule_chunks PRIMARY KEY (id)
);

CREATE TABLE item_chunks (
    id SERIAL,
    data SMALLINT[] NOT NULL,
    association_rule_set_id INT NOT NULL,
    CONSTRAINT pk_item_chunks PRIMARY KEY (id)
);

CREATE TABLE association_rule_set_metadatas (
    id SERIAL,
    last_item_chunk_index INT,
    last_rule_chunk_index INT,
    is_set_marked_to_delete BOOLEAN NOT NULL,
    set_id INT NOT NULL,
    CONSTRAINT pk_association_rule_set_metadatas PRIMARY KEY (id)
);

CREATE INDEX idx_association_rule_chunks ON association_rule_chunks (association_rule_set_id);
CREATE INDEX idx_item_chunks ON item_chunks (association_rule_set_id);
CREATE UNIQUE INDEX uq_association_rule_set_metadatas_set_id ON association_rule_set_metadatas (set_id);

-- +goose Down
DROP TABLE association_rule_set_metadatas;
DROP TABLE item_chunks;
DROP TABLE association_rule_chunks;
DROP TABLE association_rule_sets;