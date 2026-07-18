-- 01_schema.sql — базовая схема БД (UUID PK)
-- Выполнять в PostgreSQL под пользователем с правами CREATE EXTENSION/TABLE

-- CREATE EXTENSION IF NOT EXISTS pgcrypto; -- не требуется для явной генерации UUID на стороне приложения/скриптов

-- Справочник: ref_roles (роли системы)
CREATE TABLE IF NOT EXISTS ref_roles (
    id uuid PRIMARY KEY,
    code varchar(50) UNIQUE NOT NULL,
    name varchar(100) NOT NULL
);

-- Таблица: users
CREATE TABLE IF NOT EXISTS users (
    id uuid PRIMARY KEY,
    last_name varchar(150) NOT NULL,
    first_name varchar(150) NOT NULL,
    middle_name varchar(150),
    email varchar(255) UNIQUE NOT NULL,
    phone varchar(20) UNIQUE NOT NULL,
    is_external boolean DEFAULT FALSE NOT NULL,
    pep_agreement_signed boolean DEFAULT FALSE NOT NULL,
    pep_signed_at timestamp with time zone,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    -- онбординг внешних директоров и согласия ПДн
    invitation_token varchar(255),
    invitation_expires_at timestamp with time zone,
    declaration_completed boolean DEFAULT FALSE NOT NULL,
    declaration_data text,
    pdn_consent_given boolean DEFAULT FALSE NOT NULL,
    pdn_consent_at timestamp with time zone,
    pdn_consent_ip varchar(45)
);

CREATE INDEX IF NOT EXISTS idx_users_is_external ON users(is_external);

CREATE TABLE IF NOT EXISTS user_roles (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id uuid NOT NULL REFERENCES ref_roles(id) ON DELETE CASCADE,
    UNIQUE (user_id, role_id)
);

-- Таблица: committees
CREATE TABLE IF NOT EXISTS committees (
    id uuid PRIMARY KEY,
    code varchar(20) UNIQUE NOT NULL,
    name varchar(255) NOT NULL,
    description text,
    behavior_type varchar(50) NOT NULL CHECK (behavior_type IN ('CONTROL','STRATEGIC')),
    is_mandatory_for_public boolean DEFAULT FALSE NOT NULL,
    is_active boolean DEFAULT TRUE NOT NULL,
    chair_id uuid REFERENCES users(id) ON DELETE SET NULL,
    secretary_id uuid REFERENCES users(id) ON DELETE SET NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT CK_committees_chair_secretary_different CHECK (chair_id IS NULL OR secretary_id IS NULL OR chair_id <> secretary_id)
);

CREATE INDEX IF NOT EXISTS idx_committees_is_active ON committees(is_active);
CREATE INDEX IF NOT EXISTS idx_committees_behavior_type ON committees(behavior_type);

-- Связка: committee_members
CREATE TABLE IF NOT EXISTS committee_members (
    id uuid PRIMARY KEY,
    committee_id uuid NOT NULL REFERENCES committees(id) ON DELETE CASCADE,
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    UNIQUE (committee_id, user_id)
);

-- Таблица: meetings
CREATE TABLE IF NOT EXISTS meetings (
    id uuid PRIMARY KEY,
    meeting_number varchar(50),
    meeting_form varchar(20) NOT NULL CHECK (meeting_form IN ('OCHN','ZAOCHN')),
    status varchar(50) DEFAULT 'DRAFT' NOT NULL CHECK (status IN ('DRAFT','NOTIFIED','VOTING','PROTOCOL','ARCHIVE')),
    voting_start_at timestamp with time zone,
    voting_end_at timestamp with time zone,
    created_by uuid REFERENCES users(id) ON DELETE SET NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_meetings_meeting_number ON meetings(meeting_number);
CREATE INDEX IF NOT EXISTS idx_meetings_status ON meetings(status);
CREATE INDEX IF NOT EXISTS idx_meetings_created_at ON meetings(created_at);

-- Таблица: agenda_questions
CREATE TABLE IF NOT EXISTS agenda_questions (
    id uuid PRIMARY KEY,
    meeting_id uuid NOT NULL REFERENCES meetings(id) ON DELETE CASCADE,
    sequence_number int NOT NULL,
    question_text text NOT NULL,
    proposed_resolution text NOT NULL,
    status varchar(50) DEFAULT 'PENDING' NOT NULL CHECK (status IN ('PENDING','DISCUSSION','VOTED','POSTPONED'))
);

CREATE INDEX IF NOT EXISTS idx_aq_meeting_id ON agenda_questions(meeting_id);
CREATE INDEX IF NOT EXISTS idx_aq_status ON agenda_questions(status);

-- Таблица: committee_tasks
CREATE TABLE IF NOT EXISTS committee_tasks (
    id uuid PRIMARY KEY,
    committee_id uuid NOT NULL REFERENCES committees(id) ON DELETE CASCADE,
    agenda_question_id uuid REFERENCES agenda_questions(id) ON DELETE SET NULL,
    task_description text NOT NULL,
    deadline_at timestamp with time zone NOT NULL,
    status varchar(50) DEFAULT 'IN_WORK' NOT NULL CHECK (status IN ('IN_WORK','REVIEW','COMPLETED')),
    created_by uuid REFERENCES users(id) ON DELETE SET NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_ct_committee_id ON committee_tasks(committee_id);
CREATE INDEX IF NOT EXISTS idx_ct_status ON committee_tasks(status);
CREATE INDEX IF NOT EXISTS idx_ct_deadline_at ON committee_tasks(deadline_at);

-- Таблица: bulletins
CREATE TABLE IF NOT EXISTS bulletins (
    id uuid PRIMARY KEY,
    agenda_question_id uuid NOT NULL REFERENCES agenda_questions(id) ON DELETE CASCADE,
    user_id uuid REFERENCES users(id) ON DELETE SET NULL,
    vote_value varchar(15) NOT NULL CHECK (vote_value IN ('ZA','PROTIV','VOZDERZHALSYA','CONFLICT')),
    special_opinion text,
    signature_type varchar(10) NOT NULL CHECK (signature_type IN ('PEP','UKEP')),
    signature_value text NOT NULL,
    signed_at timestamp with time zone NOT NULL,
    is_cancelled boolean DEFAULT FALSE NOT NULL,
    cancellation_reason text,
    CONSTRAINT unique_vote UNIQUE (agenda_question_id, user_id, is_cancelled)
);

CREATE INDEX IF NOT EXISTS idx_b_agenda_question_id ON bulletins(agenda_question_id);
CREATE INDEX IF NOT EXISTS idx_b_user_id ON bulletins(user_id);
CREATE INDEX IF NOT EXISTS idx_b_vote_value ON bulletins(vote_value);
CREATE INDEX IF NOT EXISTS idx_b_signed_at ON bulletins(signed_at);

-- Таблица: security_audit_log (некорректируемый аудит)
CREATE TABLE IF NOT EXISTS security_audit_log (
    id bigserial PRIMARY KEY,
    user_id uuid,
    user_ip varchar(45) NOT NULL,
    action_code varchar(100) NOT NULL,
    entity_name varchar(100),
    entity_id uuid,
    description text NOT NULL,
    log_timestamp timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Справочник: ref_okopf (ОКОПФ)
CREATE TABLE IF NOT EXISTS ref_okopf (
    id uuid PRIMARY KEY,
    code varchar(10) UNIQUE NOT NULL,
    name varchar(500) NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_ref_okopf_name ON ref_okopf(name);

-- Справочник: ref_month (месяцы)
CREATE TABLE IF NOT EXISTS ref_month (
    id uuid PRIMARY KEY,
    code varchar(2) UNIQUE NOT NULL,
    name varchar(20) NOT NULL
);

-- Справочник: ref_osa_form (Форма проведения ОСА)
CREATE TABLE IF NOT EXISTS ref_osa_form (
    id uuid PRIMARY KEY,
    code varchar(10) UNIQUE NOT NULL,
    name varchar(200) NOT NULL
);

-- Таблица: osa_meetings (записи общих собраний акционеров)
CREATE TABLE IF NOT EXISTS osa_meetings (
    id uuid PRIMARY KEY,
    osa_form_id uuid NOT NULL REFERENCES ref_osa_form(id) ON DELETE RESTRICT,
    gosa_window_start date,
    gosa_window_end date,
    gosa_year int,
    shareholders_count int,
    board_min_number int,
    board_member_number int,
    executive_directors_participate boolean NOT NULL DEFAULT false,
    executive_directors_count int,
    non_executive_directors_participate boolean NOT NULL DEFAULT false,
    non_executive_directors_count int,
    independent_directors_participate boolean NOT NULL DEFAULT false,
    independent_directors_count int,
    shareholders_list_received boolean NOT NULL DEFAULT false,
    absentee_voting boolean NOT NULL DEFAULT false,
    status varchar(20) NOT NULL DEFAULT 'DRAFT',
    finalized_by uuid REFERENCES users(id),
    finalized_at timestamp with time zone,
    osa_held boolean NOT NULL DEFAULT false,
    protocol_signed boolean NOT NULL DEFAULT false,
    deputy_chair_provided boolean NOT NULL DEFAULT false,
    secretary_provided boolean NOT NULL DEFAULT true,
    secretary_signs_protocols boolean NOT NULL DEFAULT false,
    temporary_chair_provided boolean NOT NULL DEFAULT false,
    board_composition_approved boolean NOT NULL DEFAULT false,
    temporary_chair_selection varchar(50),
    temporary_chair_name varchar(300),
    protocol_signed_at timestamp with time zone,
    ballot_deadline timestamp with time zone,
    created_at timestamp with time zone DEFAULT NOW()
);

-- Таблица: osa_meeting_files (связь ОСА с файлами)
CREATE TABLE IF NOT EXISTS osa_meeting_files (
    id uuid PRIMARY KEY,
    osa_meeting_id uuid NOT NULL REFERENCES osa_meetings(id) ON DELETE CASCADE,
    file_id uuid NOT NULL REFERENCES files(id) ON DELETE CASCADE,
    file_type varchar(50) NOT NULL CHECK (file_type IN ('CHARTER','PROTOCOL','REGULATION')),
    display_name varchar(255),
    CONSTRAINT ux_osa_meeting_file UNIQUE (osa_meeting_id, file_id)
);

CREATE INDEX IF NOT EXISTS ix_omf_osa_meeting_id ON osa_meeting_files(osa_meeting_id);
CREATE INDEX IF NOT EXISTS ix_omf_file_id ON osa_meeting_files(file_id);

-- Таблица: board_members (члены СД, состав утверждается ОСА)
CREATE TABLE IF NOT EXISTS board_members (
    id uuid PRIMARY KEY,
    osa_meeting_id uuid NOT NULL REFERENCES osa_meetings(id) ON DELETE CASCADE,
    full_name varchar(300) NOT NULL,
    position varchar(200),
    board_member_type_id uuid REFERENCES ref_board_member_types(id)
);
CREATE INDEX IF NOT EXISTS ix_bm_osa_meeting_id ON board_members(osa_meeting_id);

-- Справочник: ref_board_member_types (типы директоров)
CREATE TABLE IF NOT EXISTS ref_board_member_types (
    id uuid PRIMARY KEY,
    code varchar(20) UNIQUE NOT NULL,
    name varchar(200) NOT NULL
);

-- Справочник: ref_board_roles (должности в СД)
CREATE TABLE IF NOT EXISTS ref_board_roles (
    id uuid PRIMARY KEY,
    code varchar(20) UNIQUE NOT NULL,
    name varchar(200) NOT NULL,
    sort_order int NOT NULL DEFAULT 0
);

-- Таблица: board_member_appointments (SCD Type 2 — история должностей членов СД)
CREATE TABLE IF NOT EXISTS board_member_appointments (
    id uuid PRIMARY KEY,
    board_member_id uuid NOT NULL REFERENCES board_members(id) ON DELETE CASCADE,
    role_id uuid NOT NULL REFERENCES ref_board_roles(id),
    started_at date NOT NULL,
    ended_at date,
    status varchar(20) NOT NULL DEFAULT 'ACTIVE' CHECK (status IN ('ACTIVE','COMPLETED')),
    CONSTRAINT ck_appointment_dates CHECK (ended_at IS NULL OR ended_at >= started_at)
);
CREATE INDEX IF NOT EXISTS ix_bma_member_id ON board_member_appointments(board_member_id);
CREATE INDEX IF NOT EXISTS ix_bma_role_id ON board_member_appointments(role_id);

-- Таблица: legal_entities (ЮЛ)
CREATE TABLE IF NOT EXISTS legal_entities (
    id uuid PRIMARY KEY,
    name varchar(500) NOT NULL,
    short_name varchar(255),
    inn varchar(12),
    ogrn varchar(15),
    okopf_id uuid REFERENCES ref_okopf(id) ON DELETE RESTRICT,
    shareholders_count int
);
CREATE INDEX IF NOT EXISTS ix_legal_entities_name ON legal_entities(name);
CREATE INDEX IF NOT EXISTS ix_legal_entities_inn ON legal_entities(inn);
CREATE INDEX IF NOT EXISTS ix_legal_entities_ogrn ON legal_entities(ogrn);

-- Таблица: current_workplace (руководитель ЮЛ, singleton, BDR‑007)
CREATE TABLE IF NOT EXISTS current_workplace (
    id uuid PRIMARY KEY,
    full_name varchar(300) NOT NULL,
    position varchar(200)
);

-- Singleton: не более одной записи руководителя (одно-компанийный режим, BDR‑007)
CREATE UNIQUE INDEX IF NOT EXISTS ux_current_workplace_singleton ON current_workplace ((1));

-- Таблица: notifications (уведомления)
CREATE TABLE IF NOT EXISTS notifications (
    id uuid PRIMARY KEY,
    user_id uuid REFERENCES users(id) ON DELETE SET NULL,
    committee_id uuid REFERENCES committees(id) ON DELETE SET NULL,
    meeting_id uuid REFERENCES meetings(id) ON DELETE SET NULL,
    notification_type varchar(50) NOT NULL,
    title varchar(500) NOT NULL,
    body text NOT NULL,
    is_read boolean DEFAULT FALSE NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_notifications_user_id ON notifications(user_id);
CREATE INDEX IF NOT EXISTS ix_notifications_committee_id ON notifications(committee_id);
CREATE INDEX IF NOT EXISTS ix_notifications_meeting_id ON notifications(meeting_id);
CREATE INDEX IF NOT EXISTS ix_notifications_created_at ON notifications(created_at);

-- Таблица: legal_entity_board_settings (глобальные настройки СД, singleton, BDR‑007)
CREATE TABLE IF NOT EXISTS legal_entity_board_settings (
    id uuid PRIMARY KEY,
    minimum_member_number int NOT NULL,
    member_number int NOT NULL,
    -- Интервал проведения годового общего собрания акционеров (ГОСА)
    gosa_window_start date,
    gosa_window_end date,
    -- Опции организационного устройства Совета директоров
    deputy_chair_provided boolean NOT NULL DEFAULT FALSE,
    secretary_provided boolean NOT NULL DEFAULT TRUE,
    secretary_signs_protocols boolean NOT NULL DEFAULT FALSE,
    board_mandatory boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ck_gosa_window_valid CHECK (
        (gosa_window_start IS NULL AND gosa_window_end IS NULL)
        OR (gosa_window_start IS NOT NULL AND gosa_window_end IS NOT NULL AND gosa_window_start <= gosa_window_end)
    )
);

-- Singleton: не более одной записи в таблице глобальных настроек СД
CREATE UNIQUE INDEX IF NOT EXISTS ux_board_settings_singleton ON legal_entity_board_settings ((1));

-- Таблица: files (метаданные файлов для единого файлового хранилища, ADR-020)
CREATE TABLE IF NOT EXISTS files (
    id uuid PRIMARY KEY,
    original_name varchar(255) NOT NULL,
    content_type varchar(255),
    size_bytes bigint NOT NULL CHECK (size_bytes >= 0),
    storage_provider varchar(10) NOT NULL CHECK (storage_provider IN ('LOCAL','S3')),
    storage_key_or_path varchar(1024) NOT NULL,
    checksum varchar(64), -- SHA-256 в hex (64 символа), опционально
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid REFERENCES users(id) ON DELETE SET NULL
);

-- Уникальность: один и тот же ключ хранения в пределах провайдера
CREATE UNIQUE INDEX IF NOT EXISTS ux_files_provider_key ON files(storage_provider, storage_key_or_path);

-- Полезные индексы
CREATE INDEX IF NOT EXISTS ix_files_created_at ON files(created_at);
CREATE INDEX IF NOT EXISTS ix_files_checksum ON files(checksum);
