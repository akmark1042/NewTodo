-- ---------- MIGRONDI:UP:1681432057900 --------------
-- Write your Up migrations here

CREATE TABLE todo (
    id uuid DEFAULT uuid_generate_v4() PRIMARY KEY,
    label text NOT NULL,
    completed_on timestamptz,
    created_at timestamptz NOT NULL DEFAULT (now() at time zone 'utc'),
    updated_at timestamptz NOT NULL DEFAULT (now() at time zone 'utc')
);

SELECT manage_updated_at('todo');

-- ---------- MIGRONDI:DOWN:1681432057900 --------------
-- Write how to revert the migration here

DROP TABLE todo;
