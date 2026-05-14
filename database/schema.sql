CREATE TABLE IF NOT EXISTS users (
    id uuid PRIMARY KEY,
    email varchar(200) NOT NULL UNIQUE,
    password_hash varchar(500) NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS projects (
    id uuid PRIMARY KEY,
    name varchar(200) NOT NULL,
    description varchar(1000),
    start_date timestamptz NOT NULL,
    end_date timestamptz,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS tasks (
    id uuid PRIMARY KEY,
    project_id uuid NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    title varchar(200) NOT NULL,
    content varchar(2000),
    status varchar(20) NOT NULL DEFAULT 'Todo',
    priority integer NOT NULL DEFAULT 0,
    due_date timestamptz,
    CONSTRAINT ck_tasks_status CHECK (status IN ('Todo', 'Doing', 'Done')),
    CONSTRAINT ck_tasks_priority CHECK (priority >= 0)
);

CREATE INDEX IF NOT EXISTS ix_tasks_project_id ON tasks(project_id);