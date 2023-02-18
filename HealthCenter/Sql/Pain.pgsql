DROP TABLE IF EXISTS teachers

CREATE TABLE teachers (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(255),
    last_name VARCHAR(255),
    subject VARCHAR(255),
    salary INT)