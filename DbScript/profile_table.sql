-- Table: vPayCorp.profile

-- DROP TABLE "vPayCorp".profile;

CREATE TABLE "vPayCorp".profile
(
    id integer NOT NULL DEFAULT nextval('"vPayCorp".profile_id_seq'::regclass),
    user_name character varying(20) COLLATE pg_catalog."default" NOT NULL,
    first_name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    last_name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    email_address character varying(100) COLLATE pg_catalog."default" NOT NULL,
    phone_number character varying(20) COLLATE pg_catalog."default" NOT NULL,
    "DateOfBirth" date,
    created_ts date NOT NULL,
    updated_ts date,
    password_salt bytea,
    password_hash bytea,
    CONSTRAINT profile_pk PRIMARY KEY (id),
    CONSTRAINT profile_username_unq UNIQUE (user_name)
)

TABLESPACE pg_default;

ALTER TABLE "vPayCorp".profile
    OWNER to postgres;