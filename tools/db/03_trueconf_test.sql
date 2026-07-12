-- Миграция: Тестовая страница TrueConf — заседание СД, вопросы, ответы
-- Файл: tools/db/03_trueconf_test.sql

-- Заседание СД
CREATE TABLE IF NOT EXISTS trueconf_test_meeting (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title                   VARCHAR(200) NOT NULL,
    description             TEXT,
    trueconf_conference_id  VARCHAR(100),
    trueconf_join_link      TEXT,
    conference_state        VARCHAR(50),
    started_at              TIMESTAMPTZ,
    ended_at                TIMESTAMPTZ,
    all_members_voted       BOOLEAN NOT NULL DEFAULT FALSE,
    decision_accepted       BOOLEAN,
    status                  VARCHAR(20) NOT NULL DEFAULT 'PREPARING',
    created_at              TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS ix_trueconf_test_meeting_status ON trueconf_test_meeting(status);
CREATE INDEX IF NOT EXISTS ix_trueconf_test_meeting_conference ON trueconf_test_meeting(trueconf_conference_id);

-- Вопросы повестки
CREATE TABLE IF NOT EXISTS trueconf_test_question (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    meeting_id          UUID NOT NULL REFERENCES trueconf_test_meeting(id) ON DELETE CASCADE,
    sequence_number     INT NOT NULL,
    question_text       TEXT NOT NULL,
    proposed_resolution TEXT NOT NULL DEFAULT '',
    trueconf_poll_id    VARCHAR(100),
    poll_state          VARCHAR(20),
    status              VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    created_at          TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS ix_trueconf_test_question_meeting ON trueconf_test_question(meeting_id);
CREATE INDEX IF NOT EXISTS ix_trueconf_test_question_poll ON trueconf_test_question(trueconf_poll_id);

-- Ответы (голоса)
CREATE TABLE IF NOT EXISTS trueconf_test_answer (
    id              SERIAL PRIMARY KEY,
    question_id     UUID NOT NULL REFERENCES trueconf_test_question(id) ON DELETE CASCADE,
    user_name       VARCHAR(100) NOT NULL,
    vote_value      VARCHAR(20) NOT NULL,
    voted_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS ix_trueconf_test_answer_question ON trueconf_test_answer(question_id);
