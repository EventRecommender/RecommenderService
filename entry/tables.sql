CREATE TABLE recommendation(
    userid INT NOT NULL,
    activityid INT NOT NULL,
    creationdate TIMESTAMP NOT NULL
);

CREATE TABLE interest(
    userid INT NOT NULL,
    tag VARCHAR(255) NOT NULL,
    interestvalue INT NOT NULL
);
