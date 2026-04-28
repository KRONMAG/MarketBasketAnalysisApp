-- +goose Up
CREATE TABLE association_rule_sets (
    id BIGSERIAL,
    name TEXT NOT NULL,
    description TEXT,
    transactions_count BIGINT NOT NULL,
    is_saving_complete BOOL NOT NULL,
    is_marked_to_delete BOOLEAN NOT NULL,
    created_at TIMESTAMP NOT NULL,
    CONSTRAINT pk_association_rule_sets PRIMARY KEY (id)
);

CREATE TABLE association_rule_chunks (
    id BIGSERIAL,
    data SMALLINT[] NOT NULL,
    association_rule_set_id INT NOT NULL,
    CONSTRAINT pk_association_rule_chunks PRIMARY KEY (id)
);

CREATE TABLE item_chunks (
    id BIGSERIAL,
    data SMALLINT[] NOT NULL,
    association_rule_set_id INT NOT NULL,
    CONSTRAINT pk_item_chunks PRIMARY KEY (id)
);

CREATE INDEX idx_association_rule_chunks ON association_rule_chunks (association_rule_set_id);
CREATE INDEX idx_item_chunks ON item_chunks (association_rule_set_id);

-- +goose Down
DROP TABLE item_chunks;
DROP TABLE association_rule_chunks;
DROP TABLE association_rule_sets;