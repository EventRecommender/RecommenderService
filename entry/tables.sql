CREATE TABLE recommendation(
    userid INT NOT NULL,
    tag VARCHAR(255) NOT NULL,
    amount INT NOT NULL,
    creationdate DATETIME NOT NULL
);

CREATE TABLE interest(
    userid INT NOT NULL,
    tag VARCHAR(255) NOT NULL,
    interestvalue DOUBLE NOT NULL
);
