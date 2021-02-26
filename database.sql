create table answer
(
    user_id      varchar(20) not null,
    exam_id      int         not null,
    question_num int         not null,
    answer_json  text        not null,
    answer_time  datetime(6) null,
    primary key (user_id, exam_id, question_num)
);

create table event
(
    id         int auto_increment
        primary key,
    exam_id    int         not null,
    sender     varchar(20) null,
    receipt    varchar(20) null,
    type       int         not null,
    message    text        not null,
    attachment text        null
);

create table exam
(
    id          int auto_increment
        primary key,
    name        varchar(30) not null,
    description text        null,
    start_time  datetime(6) not null,
    duration    int         not null
);

create table exam_user
(
    user_id   varchar(20) not null,
    exam_id   int         not null,
    user_role int         null,
    primary key (user_id, exam_id)
);

create table question
(
    exam_id       int  not null,
    number        int  not null,
    question_json text not null,
    primary key (number, exam_id)
);

create table user
(
    id        varchar(20) not null
        primary key,
    nick_name varchar(20) not null,
    email     varchar(30) null,
    phone     varchar(20) null,
    password  char(32)    not null,
    constraint user_email_uindex
        unique (email),
    constraint user_phone_uindex
        unique (phone)
);


