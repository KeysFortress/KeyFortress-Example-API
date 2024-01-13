CREATE EXTENSION pgcrypto;

create table if not exists accounts
(
    id       uuid default gen_random_uuid() not null
        constraint accounts_pk
            primary key,
    email    varchar(120),
    last_key uuid
);

alter table accounts
    owner to postgres;

create table if not exists user_access_keys
(
    id         uuid                 not null
        constraint user_access_keys_pk
            primary key,
    user_id    uuid
        constraint user_access_keys_accounts_id_fk
            references accounts,
    public_key text,
    enabled    boolean default true not null
);

alter table user_access_keys
    owner to postgres;
 