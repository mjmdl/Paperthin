/*

CREATE DATABASE paperthin;

CREATE SCHEMA paperthin;
CREATE SCHEMA paperthin_ext;

CREATE EXTENSION "uuid-ossp" SCHEMA paperthin_ext;

*/

CREATE OR REPLACE FUNCTION paperthin.generate_uuid()
    RETURNS UUID
    LANGUAGE SQL AS
$$
    SELECT paperthin_ext.uuid_generate_v4();
$$;



CREATE TABLE paperthin.account (
    id         UUID        NOT NULL DEFAULT paperthin.generate_uuid(),
    name       TEXT        NOT NULL,
    email      TEXT        NOT NULL,
    password   TEXT        NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,

    CONSTRAINT account_pk PRIMARY KEY (id)
);

CREATE UNIQUE INDEX account_ux_email ON paperthin.account (email) WHERE deleted_at IS NULL;



CREATE TABLE paperthin.form (
    id          UUID        NOT NULL DEFAULT paperthin.generate_uuid(),
    name        TEXT        NOT NULL,
    description TEXT,
    owned_by    UUID        NOT NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ,
    deleted_at  TIMESTAMPTZ,

    CONSTRAINT form_pk PRIMARY KEY (id),
    CONSTRAINT form_fk_owner FOREIGN KEY (owned_by) REFERENCES paperthin.account (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX form_ux_name ON paperthin.form (name, owned_by) WHERE deleted_at IS NULL;



CREATE TABLE paperthin.form_revision (
    id              UUID    NOT NULL DEFAULT paperthin.generate_uuid(),
    current_id      UUID    NOT NULL,
    next_id         UUID,
    current_version INTEGER NOT NULL,

    CONSTRAINT form_revision_pk PRIMARY KEY (id),
    CONSTRAINT form_revision_fk_current FOREIGN KEY (current_id) REFERENCES paperthin.form (id) ON DELETE CASCADE,
    CONSTRAINT form_revision_fk_next FOREIGN KEY (next_id) REFERENCES paperthin.form (id) ON DELETE SET NULL,
    CONSTRAINT form_revision_uq_current UNIQUE (current_id),
    CONSTRAINT form_revision_uq_next UNIQUE (next_id)
);



CREATE TYPE paperthin.field_variant AS ENUM (
    'option_list',
    'numeric',
    'text'
);

CREATE TABLE paperthin.field (
    id         UUID        NOT NULL DEFAULT paperthin.generate_uuid(),
    form_id    UUID        NOT NULL,
    variant    paperthin.field_variant NOT NULL,
    statement  TEXT        NOT NULL,
    position   INTEGER     NOT NULL,
    code       TEXT        NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,

    CONSTRAINT field_pk PRIMARY KEY (id),
    CONSTRAINT field_fk_form FOREIGN KEY (form_id) REFERENCES paperthin.form (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX field_ux_code ON paperthin.field (code, form_id) WHERE deleted_at IS NULL;
CREATE UNIQUE INDEX field_ux_position ON paperthin.field (position, form_id) WHERE deleted_at IS NULL;



CREATE TABLE paperthin.option_list_field (
    field_id  UUID    NOT NULL,
    n_choices INTEGER NOT NULL DEFAULT 1,

    CONSTRAINT option_list_field_pk PRIMARY KEY (field_id),
    CONSTRAINT option_list_field_fk_field FOREIGN KEY (field_id) REFERENCES paperthin.field (id) ON DELETE CASCADE
);

CREATE TABLE paperthin.option (
    id               UUID        NOT NULL DEFAULT paperthin.generate_uuid(),
    field_id         UUID        NOT NULL,
    description      TEXT        NOT NULL,
    remarks          TEXT,
    position         INTEGER     NOT NULL,
    default_selected BOOLEAN     NOT NULL DEFAULT FALSE,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ,
    deleted_at       TIMESTAMPTZ,

    CONSTRAINT option_pk PRIMARY KEY (id),
    CONSTRAINT option_fk_option_list_field FOREIGN KEY (field_id) REFERENCES paperthin.option_list_field (field_id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX option_ux_position ON paperthin.option (position, field_id) WHERE deleted_at IS NULL;



CREATE TABLE paperthin.numeric_field (
    field_id         UUID    NOT NULL,
    initial_value    NUMERIC,
    minimum_value    NUMERIC,
    maximum_value    NUMERIC,
    is_fractional    BOOLEAN NOT NULL DEFAULT TRUE,
    allowed_values   NUMERIC[],

    CONSTRAINT numeric_field_pk PRIMARY KEY (field_id),
    CONSTRAINT numeric_field_fk_field FOREIGN KEY (field_id) REFERENCES paperthin.field (id),
    CONSTRAINT numeric_field_ck_minmax CHECK (
        (minimum_value IS NULL OR maximum_value IS NULL)
	OR
	(minimum_value < maximum_value)
    )
);



CREATE TABLE paperthin.text_field (
    field_id        UUID    NOT NULL,
    initial_content TEXT,
    minimum_length  INTEGER,
    maximum_length  INTEGER,
    regexp          TEXT,
    is_multiline    BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT text_field_pk PRIMARY KEY (field_id),
    CONSTRAINT text_field_fk_field FOREIGN KEY (field_id) REFERENCES paperthin.field (id) ON DELETE CASCADE,
    CONSTRAINT text_field_ck_minmax CHECK (
        (minimum_length IS NULL OR maximum_length IS NULL)
	OR
	(minimum_length < maximum_length)
    ),
    CONSTRAINT text_field_ck_regexp CHECK (regexp IS NULL OR '' ~ regexp OR '' !~ regexp)
);



CREATE TABLE paperthin.media (
    id         UUID        NOT NULL DEFAULT paperthin.generate_uuid(),
    url        TEXT,
    filename   TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,

    CONSTRAINT media_pk PRIMARY KEY (id),
    CONSTRAINT media_ck CHECK ((url IS NULL) <> (filename IS NULL))
);

CREATE TABLE paperthin.form_media (
    media_id UUID NOT NULL,
    form_id  UUID NOT NULL,

    CONSTRAINT form_media_pk PRIMARY KEY (media_id),
    CONSTRAINT form_media_fk_media FOREIGN KEY (media_id) REFERENCES paperthin.media (id) ON DELETE CASCADE,
    CONSTRAINT form_media_fk_form FOREIGN KEY (form_id) REFERENCES paperthin.form (id) ON DELETE CASCADE
);

CREATE TABLE paperthin.field_media (
    media_id UUID NOT NULL,
    field_id UUID NOT NULL,

    CONSTRAINT field_media_pk PRIMARY KEY (media_id),
    CONSTRAINT field_media_fk_media FOREIGN KEY (media_id) REFERENCES paperthin.media (id) ON DELETE CASCADE,
    CONSTRAINT field_media_fk_field FOREIGN KEY (field_id) REFERENCES paperthin.field (id) ON DELETE CASCADE
);

CREATE TABLE paperthin.option_media (
    media_id  UUID NOT NULL,
    option_id UUID NOT NULL,

    CONSTRAINT option_media_pk PRIMARY KEY (media_id),
    CONSTRAINT option_media_fk_media FOREIGN KEY (media_id) REFERENCES paperthin.media (id) ON DELETE CASCADE,
    CONSTRAINT option_media_fk_option FOREIGN KEY (option_id) REFERENCES paperthin.option (id) ON DELETE CASCADE
);



CREATE TABLE paperthin.form_entry (
    id         UUID        NOT NULL DEFAULT paperthin.generate_uuid(),
    filled_by  UUID        NOT NULL,
    form_id    UUID        NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,

    CONSTRAINT form_entry_pk PRIMARY KEY (id),
    CONSTRAINT form_entry_fk_filler FOREIGN KEY (filled_by) REFERENCES paperthin.account (id) ON DELETE CASCADE,
    CONSTRAINT form_entry_fk_form FOREIGN KEY (form_id) REFERENCES paperthin.form (id) ON DELETE CASCADE
);



CREATE TABLE paperthin.option_selected (
    id         UUID        NOT NULL DEFAULT paperthin.generate_uuid(),
    entry_id   UUID        NOT NULL,
    option_id  UUID        NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,

    CONSTRAINT option_selected_pk PRIMARY KEY (id),
    CONSTRAINT option_selected_fk_entry FOREIGN KEY (entry_id) REFERENCES paperthin.form_entry (id),
    CONSTRAINT option_selected_fk_option FOREIGN KEY (option_id) REFERENCES paperthin.option (id)
);

CREATE UNIQUE INDEX option_selected_ux ON paperthin.option_selected (entry_id, option_id) WHERE deleted_at IS NULL;



CREATE TABLE paperthin.number_inserted (
    id         UUID        NOT NULL DEFAULT paperthin.generate_uuid(),
    entry_id   UUID        NOT NULL,
    field_id   UUID        NOT NULL,
    value      NUMERIC     NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,

    CONSTRAINT number_inserted_pk PRIMARY KEY (id),
    CONSTRAINT number_inserted_fk_entry FOREIGN KEY (entry_id) REFERENCES paperthin.form_entry (id) ON DELETE CASCADE,
    CONSTRAINT number_inserted_fk_field FOREIGN KEY (field_id) REFERENCES paperthin.field (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX number_inserted_ux ON paperthin.number_inserted (entry_id, field_id) WHERE deleted_at IS NULL;



CREATE TABLE paperthin.text_inserted (
    id         UUID        NOT NULL DEFAULT paperthin.generate_uuid(),
    entry_id   UUID        NOT NULL,
    field_id   UUID        NOT NULL,
    content    TEXT        NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,

    CONSTRAINT text_inserted_pk PRIMARY KEY (id),
    CONSTRAINT text_inserted_fk_entry FOREIGN KEY (entry_id) REFERENCES paperthin.form_entry (id) ON DELETE CASCADE,
    CONSTRAINT text_inserted_fk_field FOREIGN KEY (field_id) REFERENCES paperthin.field (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX text_inserted_ux ON paperthin.text_inserted (entry_id, field_id) WHERE deleted_at IS NULL;
