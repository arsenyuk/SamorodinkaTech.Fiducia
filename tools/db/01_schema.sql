-- 01_schema.sql — базовая схема БД (UUID PK)
-- Выполнять в PostgreSQL под пользователем с правами CREATE EXTENSION/TABLE

-- CREATE EXTENSION IF NOT EXISTS pgcrypto; -- не требуется для явной генерации UUID на стороне приложения/скриптов

-- ============================================================================
-- Пользователи (создаются первыми — persons ссылаются на users через created_by)
-- ============================================================================

-- Таблица: users
CREATE TABLE IF NOT EXISTS users (
    id uuid PRIMARY KEY,
    person_id uuid,
    login varchar(100) UNIQUE NOT NULL,
    last_name varchar(150) NOT NULL,
    first_name varchar(150) NOT NULL,
    middle_name varchar(150),
    email varchar(255) UNIQUE NOT NULL,
    phone varchar(20) UNIQUE NOT NULL,
    is_external boolean DEFAULT FALSE NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid REFERENCES users(id),
    -- онбординг внешних директоров
    invitation_token varchar(255),
    invitation_expires_at timestamp with time zone,
    -- управление учётной записью
    is_active boolean DEFAULT TRUE NOT NULL,
    account_expires_at timestamp with time zone,
    ldap_created_at timestamp with time zone,
    is_system boolean DEFAULT FALSE NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_users_is_external ON users(is_external);
CREATE INDEX IF NOT EXISTS ix_users_is_active ON users(is_active);
CREATE INDEX IF NOT EXISTS ix_users_is_system ON users(is_system);
CREATE INDEX IF NOT EXISTS ix_users_person_id ON users(person_id);

-- FK users → persons (добавляется после создания persons)
-- см. конец файла

-- ============================================================================
-- Физические лица (создаются после users — ссылаются на users через created_by)
-- ============================================================================

-- Таблица: persons (физические лица)
CREATE TABLE IF NOT EXISTS persons (
    id uuid PRIMARY KEY,
    last_name varchar(150) NOT NULL,
    first_name varchar(150) NOT NULL,
    middle_name varchar(150),
    email varchar(255) UNIQUE NOT NULL,
    phone varchar(20),
    inn varchar(12),
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

CREATE INDEX IF NOT EXISTS ix_persons_inn ON persons(inn);

-- ============================================================================
-- Добавление FK users → persons (после создания обеих таблиц)
-- ============================================================================

ALTER TABLE users ADD CONSTRAINT fk_users_person_id
    FOREIGN KEY (person_id) REFERENCES persons(id) ON DELETE SET NULL;

-- ============================================================================
-- Согласия на обработку ПДн (привязаны к ФЛ, а не к пользователю)
-- ============================================================================

-- Таблица: pdn_consents (согласия на обработку персональных данных)
CREATE TABLE IF NOT EXISTS pdn_consents (
    id uuid PRIMARY KEY,
    person_id uuid NOT NULL REFERENCES persons(id) ON DELETE RESTRICT,
    consent_given boolean DEFAULT FALSE NOT NULL,
    consent_at timestamp with time zone,
    consent_ip varchar(45),
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_pdn_consents_person_id ON pdn_consents(person_id);

-- ============================================================================
-- ПЭП: соглашение о Politically Exposed Person (привязано к ФЛ)
-- ============================================================================

-- Таблица: pep_agreements (соглашения о ПЭП)
CREATE TABLE IF NOT EXISTS pep_agreements (
    id uuid PRIMARY KEY,
    person_id uuid NOT NULL REFERENCES persons(id) ON DELETE RESTRICT,
    agreement_signed boolean DEFAULT FALSE NOT NULL,
    signed_at timestamp with time zone,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_pep_agreements_person_id ON pep_agreements(person_id);

-- ============================================================================
-- Анкета соответствия критериям независимости (привязана к ФЛ)
-- ============================================================================

-- Таблица: independence_declarations (анкеты независимости)
CREATE TABLE IF NOT EXISTS independence_declarations (
    id uuid PRIMARY KEY,
    person_id uuid NOT NULL REFERENCES persons(id) ON DELETE RESTRICT,
    hidden_shares text,
    family_connections text,
    other_boards text,
    no_criminal_record boolean DEFAULT FALSE NOT NULL,
    no_bankruptcy boolean DEFAULT FALSE NOT NULL,
    completed boolean DEFAULT FALSE NOT NULL,
    completed_at timestamp with time zone,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_independence_declarations_person_id ON independence_declarations(person_id);

-- ============================================================================
-- Справочники (ref_*): не зависят от других таблиц
-- ============================================================================

-- Справочник: ref_roles (роли системы)
CREATE TABLE IF NOT EXISTS ref_roles (
    id uuid PRIMARY KEY,
    code varchar(50) UNIQUE NOT NULL,
    name varchar(100) NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id),
    is_assignable boolean DEFAULT FALSE NOT NULL
);

-- Справочник: ref_notification_type (типы уведомлений)
CREATE TABLE IF NOT EXISTS ref_notification_type (
    id uuid PRIMARY KEY,
    code varchar(50) UNIQUE NOT NULL,
    name varchar(100) NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

CREATE INDEX IF NOT EXISTS ix_ref_notification_type_code ON ref_notification_type(code);

-- Шаблоны уведомлений (notification_template)
CREATE TABLE IF NOT EXISTS notification_template (
    id uuid PRIMARY KEY,
    notification_type_code varchar(50) UNIQUE NOT NULL REFERENCES ref_notification_type(code),
    title_template varchar(500) NOT NULL,
    body_template text NOT NULL,
    description varchar(500),
    is_enabled boolean DEFAULT TRUE NOT NULL,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_notification_template_type_code ON notification_template(notification_type_code);

-- Справочник: ref_meeting_form (формы проведения заседания СД)
CREATE TABLE IF NOT EXISTS ref_meeting_form (
    id uuid PRIMARY KEY,
    code varchar(10) UNIQUE NOT NULL,
    name varchar(200) NOT NULL,
    short_name varchar(50),
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

-- Справочник: ref_okopf (ОКОПФ)
CREATE TABLE IF NOT EXISTS ref_okopf (
    id uuid PRIMARY KEY,
    code varchar(10) UNIQUE NOT NULL,
    name varchar(500) NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

CREATE INDEX IF NOT EXISTS ix_ref_okopf_name ON ref_okopf(name);

-- Справочник: ref_standard_charter (типовые уставы ООО, Приказ № 411 от 01.08.2018)
CREATE TABLE IF NOT EXISTS ref_standard_charter (
    id uuid PRIMARY KEY,
    number varchar(2) UNIQUE NOT NULL CHECK (number::int >= 1 AND number::int <= 36),
    exit_allowed boolean NOT NULL DEFAULT false,
    transfer_to_participants_without_consent boolean NOT NULL DEFAULT false,
    transfer_to_third_parties_without_consent boolean NOT NULL DEFAULT false,
    preemptive_right boolean NOT NULL DEFAULT true,
    inheritance_without_consent boolean NOT NULL DEFAULT true,
    executive_body char(1) NOT NULL DEFAULT 'A' CHECK (executive_body IN ('A', 'B', 'C')),
    decision_confirmation_by_all_sign boolean NOT NULL DEFAULT false,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

-- Справочник: ref_month (месяцы)
CREATE TABLE IF NOT EXISTS ref_month (
    id uuid PRIMARY KEY,
    code varchar(2) UNIQUE NOT NULL,
    name varchar(20) NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

-- Справочник: ref_osa_form (Форма проведения ОСА/ОСУ)
CREATE TABLE IF NOT EXISTS ref_osa_form (
    id uuid PRIMARY KEY,
    code varchar(10) UNIQUE NOT NULL,
    name varchar(200) NOT NULL,
    short_name varchar(50),
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

-- Справочник: ref_board_of_directors_statuses (статусы Совета директоров)
CREATE TABLE IF NOT EXISTS ref_board_of_directors_statuses (
    id uuid PRIMARY KEY,
    code varchar(20) UNIQUE NOT NULL,
    name varchar(200) NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

-- Справочник: ref_board_member_types (типы директоров)
CREATE TABLE IF NOT EXISTS ref_board_member_types (
    id uuid PRIMARY KEY,
    code varchar(20) UNIQUE NOT NULL,
    name varchar(200) NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

-- Справочник: ref_board_roles (должности в СД)
CREATE TABLE IF NOT EXISTS ref_board_roles (
    id uuid PRIMARY KEY,
    code varchar(20) UNIQUE NOT NULL,
    name varchar(200) NOT NULL,
    sort_order int NOT NULL DEFAULT 0,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

-- Справочник: ref_board_member_appointment_statuses (статусы назначения членов СД)
CREATE TABLE IF NOT EXISTS ref_board_member_appointment_statuses (
    id uuid PRIMARY KEY,
    code varchar(20) UNIQUE NOT NULL,
    name varchar(200) NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

-- Справочник: ref_resignation_reasons (причины сложения полномочий)
CREATE TABLE IF NOT EXISTS ref_resignation_reasons (
    id uuid PRIMARY KEY,
    code varchar(20) UNIQUE NOT NULL,
    name varchar(200) NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

-- Справочник: ref_gd_term (сроки полномочий генерального директора ООО)
CREATE TABLE IF NOT EXISTS ref_gd_term (
    id uuid PRIMARY KEY,
    code varchar(20) UNIQUE NOT NULL,
    name varchar(200) NOT NULL,
    duration_years int,           -- NULL = безсрочно
    sort_order int NOT NULL DEFAULT 0,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

CREATE TABLE IF NOT EXISTS ref_measurement_unit (
    id uuid PRIMARY KEY,
    code varchar(20) UNIQUE NOT NULL,
    name varchar(200) NOT NULL,
    short_name varchar(50) NOT NULL,
    sort_order int NOT NULL DEFAULT 0,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

-- Справочник: ref_request_type (типы требований участников)
CREATE TABLE IF NOT EXISTS ref_request_type (
    id uuid PRIMARY KEY,
    code varchar(50) UNIQUE NOT NULL,
    name varchar(300) NOT NULL,
    is_for_llc boolean NOT NULL DEFAULT false,
    is_for_njsc boolean NOT NULL DEFAULT false,
    is_for_pjsc boolean NOT NULL DEFAULT false,
    requires_file boolean NOT NULL DEFAULT false,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS user_roles (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    role_id uuid NOT NULL REFERENCES ref_roles(id) ON DELETE RESTRICT,
    UNIQUE (user_id, role_id)
);

-- ============================================================================
-- Комитеты
-- ============================================================================

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
    created_by uuid REFERENCES users(id),
    CONSTRAINT ck_committees_chair_secretary_different CHECK (chair_id IS NULL OR secretary_id IS NULL OR chair_id <> secretary_id)
);

CREATE INDEX IF NOT EXISTS ix_committees_is_active ON committees(is_active);
CREATE INDEX IF NOT EXISTS ix_committees_behavior_type ON committees(behavior_type);

-- Связка: committee_members
CREATE TABLE IF NOT EXISTS committee_members (
    id uuid PRIMARY KEY,
    committee_id uuid NOT NULL REFERENCES committees(id) ON DELETE RESTRICT,
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    UNIQUE (committee_id, user_id)
);

-- ============================================================================
-- Заседания СД (board meetings)
-- ============================================================================

-- Таблица: meetings
CREATE TABLE IF NOT EXISTS meetings (
    id uuid PRIMARY KEY,
    meeting_number varchar(50),
    meeting_form_id uuid NOT NULL REFERENCES ref_meeting_form(id) ON DELETE RESTRICT,
    status varchar(50) DEFAULT 'DRAFT' NOT NULL CHECK (status IN ('DRAFT','NOTIFIED','VOTING','PROTOCOL','ARCHIVE')),
    voting_start_at timestamp with time zone,
    voting_end_at timestamp with time zone,
    created_by uuid REFERENCES users(id) ON DELETE SET NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_meetings_meeting_number ON meetings(meeting_number);
CREATE INDEX IF NOT EXISTS ix_meetings_status ON meetings(status);
CREATE INDEX IF NOT EXISTS ix_meetings_created_at ON meetings(created_at);

-- Таблица: agenda_questions
CREATE TABLE IF NOT EXISTS agenda_questions (
    id uuid PRIMARY KEY,
    meeting_id uuid NOT NULL REFERENCES meetings(id) ON DELETE RESTRICT,
    sequence_number int NOT NULL,
    question_text text NOT NULL,
    proposed_resolution text NOT NULL,
    status varchar(50) DEFAULT 'PENDING' NOT NULL CHECK (status IN ('PENDING','DISCUSSION','VOTED','POSTPONED'))
);

CREATE INDEX IF NOT EXISTS ix_agenda_questions_meeting_id ON agenda_questions(meeting_id);
CREATE INDEX IF NOT EXISTS ix_agenda_questions_status ON agenda_questions(status);

-- Таблица: committee_tasks
CREATE TABLE IF NOT EXISTS committee_tasks (
    id uuid PRIMARY KEY,
    committee_id uuid NOT NULL REFERENCES committees(id) ON DELETE RESTRICT,
    agenda_question_id uuid REFERENCES agenda_questions(id) ON DELETE SET NULL,
    task_description text NOT NULL,
    deadline_at timestamp with time zone NOT NULL,
    status varchar(50) DEFAULT 'IN_WORK' NOT NULL CHECK (status IN ('IN_WORK','REVIEW','COMPLETED')),
    created_by uuid REFERENCES users(id) ON DELETE SET NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_committee_tasks_committee_id ON committee_tasks(committee_id);
CREATE INDEX IF NOT EXISTS ix_committee_tasks_status ON committee_tasks(status);
CREATE INDEX IF NOT EXISTS ix_committee_tasks_deadline_at ON committee_tasks(deadline_at);

-- Таблица: bulletins
CREATE TABLE IF NOT EXISTS bulletins (
    id uuid PRIMARY KEY,
    agenda_question_id uuid NOT NULL REFERENCES agenda_questions(id) ON DELETE RESTRICT,
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    vote_value varchar(15) NOT NULL CHECK (vote_value IN ('ZA','PROTIV','VOZDERZHALSYA','CONFLICT')),
    special_opinion text,
    signature_type varchar(10) NOT NULL CHECK (signature_type IN ('PEP','UKEP')),
    signature_value text NOT NULL,
    signed_at timestamp with time zone NOT NULL,
    is_cancelled boolean DEFAULT FALSE NOT NULL,
    cancellation_reason text,
    CONSTRAINT ux_bulletins_question_user_cancelled UNIQUE (agenda_question_id, user_id, is_cancelled)
);

CREATE INDEX IF NOT EXISTS ix_bulletins_agenda_question_id ON bulletins(agenda_question_id);
CREATE INDEX IF NOT EXISTS ix_bulletins_user_id ON bulletins(user_id);
CREATE INDEX IF NOT EXISTS ix_bulletins_vote_value ON bulletins(vote_value);
CREATE INDEX IF NOT EXISTS ix_bulletins_signed_at ON bulletins(signed_at);

-- ============================================================================
-- Файлы (единое хранилище, ADR-020, BDR-011)
-- Должны быть созданы ДО таблиц, ссылающихся на files
-- ============================================================================

-- Таблица: files (метаданные файлов для единого файлового хранилища, ADR-020, BDR-011)
CREATE TABLE IF NOT EXISTS files (
    id uuid PRIMARY KEY,
    original_name varchar(255) NOT NULL,
    content_type varchar(255),
    size_bytes bigint NOT NULL CHECK (size_bytes >= 0),
    storage_provider varchar(10) NOT NULL CHECK (storage_provider IN ('LOCAL','S3')),
    storage_key_or_path varchar(1024) NOT NULL,
    checksum varchar(64), -- SHA-256 в hex (64 символа), опционально
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid REFERENCES users(id) ON DELETE SET NULL,
    -- Контекст использования файла (BDR-011)
    file_type varchar(50),
    display_name varchar(255),
    extension varchar(20),
    -- Chunked upload (BDR-011)
    is_uploaded boolean NOT NULL DEFAULT true,
    upload_id varchar(64),
    expires_at timestamp with time zone
);

-- Уникальность: один и тот же ключ хранения в пределах провайдера
CREATE UNIQUE INDEX IF NOT EXISTS ux_files_provider_key ON files(storage_provider, storage_key_or_path);

-- Полезные индексы
CREATE INDEX IF NOT EXISTS ix_files_created_at ON files(created_at);
CREATE INDEX IF NOT EXISTS ix_files_checksum ON files(checksum);
CREATE INDEX IF NOT EXISTS ix_files_upload_id ON files(upload_id) WHERE upload_id IS NOT NULL;

-- ============================================================================
-- Юридические лица
-- ============================================================================

-- Таблица: legal_entities (ЮЛ)
CREATE TABLE IF NOT EXISTS legal_entities (
    id uuid PRIMARY KEY,
    name varchar(500) NOT NULL,
    short_name varchar(255),
    inn varchar(12),
    ogrn varchar(15),
    okopf_id uuid REFERENCES ref_okopf(id) ON DELETE RESTRICT,
    standard_charter_id uuid REFERENCES ref_standard_charter(id) ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS ix_legal_entities_name ON legal_entities(name);
CREATE INDEX IF NOT EXISTS ix_legal_entities_inn ON legal_entities(inn);
CREATE INDEX IF NOT EXISTS ix_legal_entities_ogrn ON legal_entities(ogrn);

-- Параметры устава ООО (1:1 с legal_entities, обслуживает и типовой и нетиповой)
CREATE TABLE IF NOT EXISTS legal_entity_charter (
    legal_entity_id uuid PRIMARY KEY REFERENCES legal_entities(id) ON DELETE RESTRICT,
    exit_allowed boolean NOT NULL DEFAULT false,
    transfer_to_participants_without_consent boolean NOT NULL DEFAULT true,
    transfer_to_third_parties_without_consent boolean NOT NULL DEFAULT false,
    preemptive_right boolean NOT NULL DEFAULT true,
    inheritance_without_consent boolean NOT NULL DEFAULT true,
    executive_body char(1) NOT NULL DEFAULT 'A',
    decision_confirmation_by_all_sign boolean NOT NULL DEFAULT false,
    charter_document_id uuid REFERENCES files(id),
    board_regulation_document_id uuid REFERENCES files(id),
    committee_regulation_document_id uuid REFERENCES files(id),
    mandatory_audit boolean,
    has_revision_commission boolean,
    has_board_of_directors boolean NOT NULL DEFAULT false,
    gd_term_id uuid REFERENCES ref_gd_term(id) ON DELETE SET NULL,
    vosu_threshold_percent numeric(4,2) CHECK (vosu_threshold_percent > 0 AND vosu_threshold_percent <= 10)
);

-- Таблица: legal_entity_email_settings (настройки email-писем для ЮЛ)
CREATE TABLE IF NOT EXISTS legal_entity_email_settings (
    id uuid PRIMARY KEY,
    legal_entity_id uuid NOT NULL REFERENCES legal_entities(id) ON DELETE RESTRICT,
    header_enabled boolean NOT NULL DEFAULT false,
    header_markdown text NOT NULL DEFAULT '',
    footer_enabled boolean NOT NULL DEFAULT false,
    footer_markdown text NOT NULL DEFAULT '',
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS ix_legal_entity_email_settings_legal_entity_id ON legal_entity_email_settings(legal_entity_id);

-- Таблица: current_workplace (руководитель ЮЛ, singleton, BDR-007)
CREATE TABLE IF NOT EXISTS current_workplace (
    id uuid PRIMARY KEY,
    full_name varchar(300) NOT NULL,
    position varchar(200),
    last_selected_legal_entity_id uuid REFERENCES legal_entities(id)
);

-- Singleton: не более одной записи руководителя (одно-компанийный режим, BDR-007)
CREATE UNIQUE INDEX IF NOT EXISTS ux_current_workplace_singleton ON current_workplace ((1));

-- Таблица: legal_entity_board_settings (глобальные настройки СД, singleton, BDR-007)
CREATE TABLE IF NOT EXISTS legal_entity_board_settings (
    id uuid PRIMARY KEY,
    -- Интервал проведения годового общего собрания акционеров (ГОСА)
    gosa_window_start date,
    gosa_window_end date,
    -- Опции организационного устройства Совета директоров
    deputy_chair_provided boolean NOT NULL DEFAULT FALSE,
    secretary_provided boolean NOT NULL DEFAULT TRUE,
    secretary_signs_protocols boolean NOT NULL DEFAULT FALSE,
    -- Настройки комитетов Совета директоров
    committees_mandatory boolean NOT NULL DEFAULT FALSE,
    committees_defined_by_documents boolean NOT NULL DEFAULT FALSE,
    max_committees_per_member_defined boolean NOT NULL DEFAULT FALSE,
    max_committees_per_member int,
    max_committees_headed_per_member_defined boolean NOT NULL DEFAULT FALSE,
    max_committees_headed_per_member int,
    min_committee_members_defined boolean NOT NULL DEFAULT FALSE,
    min_committee_members int,
    committee_quorum_defined boolean NOT NULL DEFAULT FALSE,
    committee_quorum_percent int,
    joint_committee_quorum_defined boolean NOT NULL DEFAULT FALSE,
    joint_committee_quorum_percent int,
    CONSTRAINT ck_gosa_window_valid CHECK (
        (gosa_window_start IS NULL AND gosa_window_end IS NULL)
        OR (gosa_window_start IS NOT NULL AND gosa_window_end IS NOT NULL AND gosa_window_start <= gosa_window_end)
    )
);

-- Singleton: не более одной записи в таблице глобальных настроек СД
CREATE UNIQUE INDEX IF NOT EXISTS ux_board_settings_singleton ON legal_entity_board_settings ((1));

-- Таблица: legal_entity_voting_rules (правила голосования в СД, индивидуальные для ЮЛ)
CREATE TABLE IF NOT EXISTS legal_entity_voting_rules (
    id uuid PRIMARY KEY,
    legal_entity_id uuid NOT NULL REFERENCES legal_entities(id) ON DELETE RESTRICT,
    quorum_percent int NOT NULL DEFAULT 50 CHECK (quorum_percent > 0 AND quorum_percent <= 100),
    chair_tiebreaker boolean NOT NULL DEFAULT FALSE,
    absentee_opinions boolean NOT NULL DEFAULT FALSE,
    qualified_majority_percent int NOT NULL DEFAULT 75 CHECK (qualified_majority_percent > 0 AND qualified_majority_percent <= 100),
    in_person_allowed boolean NOT NULL DEFAULT TRUE,
    absentee_allowed boolean NOT NULL DEFAULT FALSE,
    mixed_allowed boolean NOT NULL DEFAULT FALSE,
    document_flow int NOT NULL DEFAULT 0,
    spot_by_election boolean NOT NULL DEFAULT FALSE,
    first_meeting_deadline_days int NOT NULL DEFAULT 30,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_voting_rules_entity ON legal_entity_voting_rules (legal_entity_id);

-- ============================================================================
-- Общие собрания акционеров/участников (ОСА/ОСУ)
-- ============================================================================

-- Таблица: osa_meetings (записи общих собраний акционеров/участников)
CREATE TABLE IF NOT EXISTS osa_meetings (
    id uuid PRIMARY KEY,
    legal_entity_id uuid NOT NULL REFERENCES legal_entities(id) ON DELETE RESTRICT,
    osa_form_id uuid NOT NULL REFERENCES ref_osa_form(id) ON DELETE RESTRICT,
    title varchar(500),
    gosa_window_start date,
    gosa_window_end date,
    election_year int,
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
    board_mandatory boolean NOT NULL DEFAULT false,
    board_approved boolean NOT NULL DEFAULT false,
    temporary_chair_selection varchar(50),
    temporary_chair_name varchar(300),
    protocol_signed_at timestamp with time zone,
    ballot_deadline timestamp with time zone,
    created_at timestamp with time zone DEFAULT NOW()
);

-- Таблица: osa_meeting_files (связь ОСА с файлами)
CREATE TABLE IF NOT EXISTS osa_meeting_files (
    id uuid PRIMARY KEY,
    osa_meeting_id uuid NOT NULL REFERENCES osa_meetings(id) ON DELETE RESTRICT,
    file_id uuid NOT NULL REFERENCES files(id) ON DELETE RESTRICT,
    CONSTRAINT ux_osa_meeting_file UNIQUE (osa_meeting_id, file_id)
);

CREATE INDEX IF NOT EXISTS ix_omf_osa_meeting_id ON osa_meeting_files(osa_meeting_id);
CREATE INDEX IF NOT EXISTS ix_omf_file_id ON osa_meeting_files(file_id);

-- Дополнительные настройки ЮЛ (1:1 с legal_entities)
CREATE TABLE IF NOT EXISTS legal_entity_extra_settings (
    legal_entity_id uuid PRIMARY KEY REFERENCES legal_entities(id) ON DELETE RESTRICT,
    -- Ведение списка участников через нотариат (ст. 31.1 14-ФЗ)
    notary_list_approved boolean NOT NULL DEFAULT false,
    notary_list_osa_meeting_id uuid REFERENCES osa_meetings(id) ON DELETE SET NULL,
    notary_list_decision_date date
);

-- Настройки доступности документов для ЮЛ (какие типы документов доступны в электронном виде)
CREATE TABLE IF NOT EXISTS legal_entity_document_access (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    legal_entity_id uuid NOT NULL REFERENCES legal_entities(id) ON DELETE CASCADE,
    document_type_code varchar(50) NOT NULL,
    is_electronic_available boolean NOT NULL DEFAULT FALSE,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    UNIQUE (legal_entity_id, document_type_code)
);

-- ============================================================================
-- Совет директоров
-- ============================================================================

-- Таблица: board_of_directors (головная запись состава СД)
CREATE TABLE IF NOT EXISTS board_of_directors (
    id uuid PRIMARY KEY,
    osa_meeting_id uuid NOT NULL REFERENCES osa_meetings(id) ON DELETE RESTRICT,
    status_id uuid NOT NULL REFERENCES ref_board_of_directors_statuses(id),
    election_year integer,
    started_at date,
    ended_at date
);

-- Таблица: board_members (члены СД, состав утверждается ОСА)
CREATE TABLE IF NOT EXISTS board_members (
    id uuid PRIMARY KEY,
    osa_meeting_id uuid NOT NULL REFERENCES osa_meetings(id) ON DELETE RESTRICT,
    board_of_directors_id uuid REFERENCES board_of_directors(id),
    full_name varchar(300) NOT NULL,
    board_member_type_id uuid REFERENCES ref_board_member_types(id),
    account varchar(100),
    email varchar(200),
    user_id uuid
);
CREATE INDEX IF NOT EXISTS ix_bm_osa_meeting_id ON board_members(osa_meeting_id);

-- Таблица: board_member_appointments (SCD Type 2 — история должностей членов СД)
CREATE TABLE IF NOT EXISTS board_member_appointments (
    id uuid PRIMARY KEY,
    board_member_id uuid NOT NULL REFERENCES board_members(id) ON DELETE RESTRICT,
    role_id uuid REFERENCES ref_board_roles(id),
    role_code varchar(20) NOT NULL,
    started_at date NOT NULL,
    ended_at date,
    status_id uuid NOT NULL REFERENCES ref_board_member_appointment_statuses(id),
    resigned_at timestamp with time zone,
    resignation_reason_id uuid REFERENCES ref_resignation_reasons(id),
    legal_basis text,
    CONSTRAINT ck_appointment_dates CHECK (ended_at IS NULL OR ended_at >= started_at),
    CONSTRAINT ck_resignation_fields CHECK (
        (status_id = '6e6bcad9-c361-48a2-9f08-3f86dbab7dc6'::uuid AND resigned_at IS NOT NULL AND resignation_reason_id IS NOT NULL)
        OR (status_id <> '6e6bcad9-c361-48a2-9f08-3f86dbab7dc6'::uuid)
    )
);
CREATE INDEX IF NOT EXISTS ix_bma_member_id ON board_member_appointments(board_member_id);
CREATE INDEX IF NOT EXISTS ix_bma_role_id ON board_member_appointments(role_id);
CREATE INDEX IF NOT EXISTS ix_bma_role_code ON board_member_appointments(role_code);

-- ============================================================================
-- Уведомления
-- ============================================================================

-- Таблица: notifications (уведомления)
CREATE TABLE IF NOT EXISTS notifications (
    id uuid PRIMARY KEY,
    user_id uuid REFERENCES users(id) ON DELETE SET NULL,
    committee_id uuid REFERENCES committees(id) ON DELETE SET NULL,
    meeting_id uuid REFERENCES meetings(id) ON DELETE SET NULL,
    notification_type varchar(50) NOT NULL,
    title varchar(500) NOT NULL,
    body text NOT NULL,
    url varchar(1000),
    is_read boolean DEFAULT FALSE NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_notifications_user_id ON notifications(user_id);
CREATE INDEX IF NOT EXISTS ix_notifications_committee_id ON notifications(committee_id);
CREATE INDEX IF NOT EXISTS ix_notifications_meeting_id ON notifications(meeting_id);
CREATE INDEX IF NOT EXISTS ix_notifications_created_at ON notifications(created_at);

-- ============================================================================
-- Повестка и предложения (agenda)
-- ============================================================================

-- Таблица: agenda_proposals (предложения от пользователей)
CREATE TABLE IF NOT EXISTS agenda_proposals (
    id uuid PRIMARY KEY,
    submitter_name varchar(300) NOT NULL,
    submitter_email varchar(300),
    proposal_text text NOT NULL,
    status varchar(20) NOT NULL DEFAULT 'SUBMITTED',
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Таблица: agenda_items (повестка совета директоров)
CREATE TABLE IF NOT EXISTS agenda_items (
    id uuid PRIMARY KEY,
    board_of_directors_id uuid NOT NULL REFERENCES board_of_directors(id),
    legal_entity_id uuid REFERENCES legal_entities(id),
    share_request_id uuid REFERENCES share_request(id) ON DELETE SET NULL,
    title text NOT NULL,
    target_type varchar(20) NOT NULL,
    reason text NOT NULL,
    status varchar(20) NOT NULL DEFAULT 'PENDING',
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- ============================================================================
-- Выборы в совет директоров (elections)
-- ============================================================================

-- Таблица: election_proposals (предложения по выборам в СД)
CREATE TABLE IF NOT EXISTS election_proposals (
    id uuid PRIMARY KEY,
    board_of_directors_id uuid NOT NULL REFERENCES board_of_directors(id),
    position varchar(20) NOT NULL,
    status varchar(20) NOT NULL DEFAULT 'OPEN',
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Таблица: election_candidacies (кандидатуры в СД)
CREATE TABLE IF NOT EXISTS election_candidacies (
    id uuid PRIMARY KEY,
    proposal_id uuid NOT NULL REFERENCES election_proposals(id) ON DELETE RESTRICT,
    candidate_member_id uuid NOT NULL REFERENCES board_members(id),
    confirmed_by_member_id uuid REFERENCES board_members(id),
    confirmed_at timestamp with time zone,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Таблица: election_consents (согласие/отказ кандидата на выборы в СД)
CREATE TABLE IF NOT EXISTS election_consents (
    id uuid PRIMARY KEY,
    proposal_id uuid NOT NULL REFERENCES election_proposals(id) ON DELETE RESTRICT,
    candidate_member_id uuid NOT NULL REFERENCES board_members(id),
    consent_given boolean NOT NULL,
    consent_token varchar(64) NOT NULL,
    signed_at timestamp with time zone,
    signed_ip varchar(45),
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_election_consents_proposal_member ON election_consents(proposal_id, candidate_member_id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_election_consents_token ON election_consents(consent_token);
CREATE INDEX IF NOT EXISTS ix_election_consents_proposal ON election_consents(proposal_id);

-- ============================================================================
-- Сложение полномочий (resignations)
-- ============================================================================

-- Таблица: user_board_member_resignations (сложение полномочий членов СД)
CREATE TABLE IF NOT EXISTS user_board_member_resignations (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    board_member_appointment_id uuid NOT NULL REFERENCES board_member_appointments(id) ON DELETE RESTRICT,
    resigned_at timestamp with time zone NOT NULL,
    resignation_reason_id uuid NOT NULL REFERENCES ref_resignation_reasons(id),
    rdl_extract_file_id uuid REFERENCES files(id),
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- ============================================================================
-- Аудит безопасности (security audit log)
-- ============================================================================

-- Таблица: security_audit_log (аудит-лог безопасности)
-- Примечание: bigint PK — наследуемая таблица, создана до BDR-004 (UUID PK)
CREATE TABLE IF NOT EXISTS security_audit_log (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id uuid,
    user_ip varchar(45) NOT NULL,
    action_code varchar(100) NOT NULL,
    entity_name varchar(100),
    entity_id uuid,
    description text NOT NULL,
    log_timestamp timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- ============================================================================
-- ext_ таблицы: данные из внешних источников (BDR-009)
-- Не являются авторитетным источником. Обновляются только через API.
-- ============================================================================

-- ext_spark_company: карточка компании из СПАРК (Интерфакс)
CREATE TABLE IF NOT EXISTS ext_spark_company (
    id uuid PRIMARY KEY,
    inn varchar(12) NOT NULL,
    ogrn varchar(15),
    full_name varchar(500),
    short_name varchar(255),
    okopf_code varchar(10),
    okopf_name varchar(255),
    legal_address text,
    status varchar(100),
    registration_date date,
    shareholders_count integer,
    employees_count integer,
    fetched_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_ext_spark_company_inn ON ext_spark_company(inn);
CREATE INDEX IF NOT EXISTS ix_ext_spark_company_fetched_at ON ext_spark_company(fetched_at);

-- ext_spark_manager: руководитель компании из СПАРК
CREATE TABLE IF NOT EXISTS ext_spark_manager (
    id uuid PRIMARY KEY,
    inn varchar(12) NOT NULL,
    full_name varchar(300) NOT NULL,
    position varchar(200),
    person_inn varchar(12),
    start_date date,
    fetched_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_ext_spark_manager_inn ON ext_spark_manager(inn);
CREATE INDEX IF NOT EXISTS ix_ext_spark_manager_fetched_at ON ext_spark_manager(fetched_at);

-- ext_spark_founder: учредители (участники) компании из СПАРК
CREATE TABLE IF NOT EXISTS ext_spark_founder (
    id uuid PRIMARY KEY,
    inn varchar(12) NOT NULL,
    name varchar(500),
    founder_inn varchar(12),
    founder_ogrn varchar(15),
    country varchar(100),
    is_foreign boolean DEFAULT FALSE,
    full_name varchar(300),
    person_inn varchar(12),
    citizenship varchar(100),
    head_of_other integer,
    founder_of_other integer,
    is_entrepreneur boolean DEFAULT FALSE,
    ogrnip varchar(15),
    share_amount numeric(18,2),
    share_percent numeric(5,2),
    entry_date date,
    exit_date date,
    fetched_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_ext_spark_founder_inn ON ext_spark_founder(inn);
CREATE INDEX IF NOT EXISTS ix_ext_spark_founder_fetched_at ON ext_spark_founder(fetched_at);

-- ============================================================================
-- Данные ЦБ РФ (FinOrg) — внешний кэш справочника участников финансового рынка
-- Источник: SOAP-сервис cbr.ru/FO_ZoomWS/FinOrg.asmx
-- TTL кэша: 24 часа (fetched_at)
-- ============================================================================

-- ext_cbr_finorg_organization: карточка организации из ЦБ РФ
CREATE TABLE IF NOT EXISTS ext_cbr_finorg_organization (
    id          uuid PRIMARY KEY,
    inn         varchar(12) NOT NULL,
    cbr_id      bigint,
    ogrn        varchar(15),
    full_name   varchar(500),
    short_name  varchar(255),
    eng_name    varchar(500),
    address     text,
    phones      varchar(500),
    email       varchar(255),
    okato       integer,
    region      varchar(255),
    fo_types    varchar(500),
    status      varchar(50) NOT NULL DEFAULT '',
    is_sro_member boolean NOT NULL DEFAULT false,
    is_rss      boolean NOT NULL DEFAULT false,
    is_npo      boolean NOT NULL DEFAULT false,
    is_asv      boolean NOT NULL DEFAULT false,
    reg_number  integer,
    bic         varchar(20),
    bank_status varchar(100),
    registration_date timestamptz,
    has_branches boolean NOT NULL DEFAULT false,
    fund_value  numeric(18,2),
    web_sites   varchar(1000),
    error       text,
    fetched_at  timestamptz NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_ext_cbr_finorg_organization_inn ON ext_cbr_finorg_organization(inn);
CREATE INDEX IF NOT EXISTS ix_ext_cbr_finorg_organization_fetched_at ON ext_cbr_finorg_organization(fetched_at);

-- ext_cbr_finorg_license: лицензии организации из ЦБ РФ
CREATE TABLE IF NOT EXISTS ext_cbr_finorg_license (
    id              uuid PRIMARY KEY,
    organization_inn varchar(12) NOT NULL,
    vid_id          integer NOT NULL,
    activity_name   varchar(500),
    number          varchar(100),
    name            varchar(255),
    start_date      timestamptz,
    end_date        timestamptz,
    fetched_at      timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_ext_cbr_finorg_license_organization_inn ON ext_cbr_finorg_license(organization_inn);
CREATE INDEX IF NOT EXISTS ix_ext_cbr_finorg_license_fetched_at ON ext_cbr_finorg_license(fetched_at);

-- ============================================================================
-- Договоры АО с регистраторами и информационными агентствами
-- ============================================================================

-- ao_contractors: договоры АО с регистраторами и информационными агентствами
CREATE TABLE IF NOT EXISTS ao_contractors (
    id                                uuid PRIMARY KEY,
    legal_entity_id                   uuid NOT NULL REFERENCES legal_entities(id) ON DELETE RESTRICT,
    contractor_inn                    varchar(12) NOT NULL,
    contractor_name                   varchar(500) NOT NULL,
    contractor_type                   varchar(20) NOT NULL,
    contract_number                   varchar(100),
    contract_date                     date,
    contract_valid_from               date,
    contract_valid_to                 date,
    is_indefinite                     boolean NOT NULL DEFAULT true,
    contract_document_id              uuid REFERENCES files(id) ON DELETE SET NULL,
    registry_preparation_days         integer,
    registry_preparation_unit         uuid REFERENCES ref_measurement_unit(id),
    dividend_registry_preparation_days integer,
    dividend_registry_preparation_unit uuid REFERENCES ref_measurement_unit(id),
    registry_rules_url                varchar(1000),
    is_active                         boolean NOT NULL DEFAULT true,
    created_at                        timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by                        uuid
);

CREATE INDEX IF NOT EXISTS ix_ao_contractors_legal_entity_id ON ao_contractors(legal_entity_id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_ao_contractors_le_type_active
    ON ao_contractors(legal_entity_id, contractor_type)
    WHERE is_active = true;

-- ============================================================================
-- Шаблоны организационных мероприятий (Org Templates)
-- Иерархия: tpl_org_intents → tpl_org_stages → tpl_org_offers (офер = шаблон задачи)
-- ============================================================================

-- tpl_org_intents: цели (верхний уровень)
CREATE TABLE IF NOT EXISTS tpl_org_intents (
    id uuid PRIMARY KEY,
    code varchar(50),
    name varchar(300) NOT NULL,
    description text,
    sort_order int NOT NULL DEFAULT 0,
    is_for_ao boolean,
    is_for_llc boolean,
    requires_board_of_directors boolean,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid REFERENCES users(id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_tpl_org_intents_code ON tpl_org_intents(code);

-- tpl_org_stages: этапы (привязаны к целям)
CREATE TABLE IF NOT EXISTS tpl_org_stages (
    id uuid PRIMARY KEY,
    intent_id uuid NOT NULL REFERENCES tpl_org_intents(id) ON DELETE RESTRICT,
    name varchar(300) NOT NULL,
    description text,
    sort_order int NOT NULL DEFAULT 0,
    start_offset_days int,
    deadline_rule varchar(100),
    deadline_days int,
    measurement_unit_id uuid REFERENCES ref_measurement_unit(id),
    dependency_type varchar(10) NOT NULL DEFAULT 'FS',
    predecessor_stage_ids text,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid REFERENCES users(id)
);
CREATE INDEX IF NOT EXISTS ix_tpl_org_stages_intent ON tpl_org_stages(intent_id);

-- tpl_org_offers: шаблоны задач (привязаны к этапам)
CREATE TABLE IF NOT EXISTS tpl_org_offers (
    id uuid PRIMARY KEY,
    stage_id uuid NOT NULL REFERENCES tpl_org_stages(id) ON DELETE RESTRICT,
    name varchar(300) NOT NULL,
    description text,
    start_offset_days int,
    deadline_rule varchar(100),
    deadline_days int,
    measurement_unit_id uuid REFERENCES ref_measurement_unit(id),
    assigned_role_id uuid REFERENCES ref_roles(id),
    assigned_board_role_id uuid REFERENCES ref_board_roles(id),
    require_notary_confirmation boolean,
    require_all_sign_confirmation boolean,
    require_committees boolean,
    require_board_regulation boolean,
    require_custom_charter boolean,
    require_executive_body_a boolean,
    require_board_of_directors boolean,
    require_document_flow_legal_electronic boolean,
    require_mandatory_audit boolean,
    require_revision_commission boolean,
    dependency_type varchar(10) NOT NULL DEFAULT 'FS',
    predecessor_offer_ids text,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid REFERENCES users(id)
);
CREATE INDEX IF NOT EXISTS ix_tpl_org_offers_stage ON tpl_org_offers(stage_id);
CREATE INDEX IF NOT EXISTS ix_tpl_org_offers_assigned_role ON tpl_org_offers(assigned_role_id);
CREATE INDEX IF NOT EXISTS ix_tpl_org_offers_board_role ON tpl_org_offers(assigned_board_role_id);

-- tpl_org_milestones: шаблоны вех (привязаны к целям/этапам)
CREATE TABLE IF NOT EXISTS tpl_org_milestones (
    id uuid PRIMARY KEY,
    intent_id uuid NOT NULL REFERENCES tpl_org_intents(id) ON DELETE RESTRICT,
    stage_id uuid REFERENCES tpl_org_stages(id) ON DELETE RESTRICT,
    name varchar(300) NOT NULL,
    description text,
    milestone_type varchar(20) NOT NULL,
    predecessor_offer_ids text,
    predecessor_stage_ids text,
    offset_days int,
    measurement_unit_id uuid REFERENCES ref_measurement_unit(id),
    control_offer_id uuid REFERENCES tpl_org_offers(id) ON DELETE RESTRICT,
    legal_reference varchar(500),
    sort_order int NOT NULL DEFAULT 0,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid REFERENCES users(id)
);
CREATE INDEX IF NOT EXISTS ix_tpl_org_milestones_intent ON tpl_org_milestones(intent_id);
CREATE INDEX IF NOT EXISTS ix_tpl_org_milestones_stage ON tpl_org_milestones(stage_id);

-- ============================================================================
-- Реальные планы организационных мероприятий (создаются из шаблонов tpl_org_*)
-- Привязываются к конкретному ЮЛ, имеют фактические даты и статус выполнения
-- ============================================================================

CREATE TABLE IF NOT EXISTS org_intents (
    id uuid PRIMARY KEY,
    legal_entity_id uuid NOT NULL REFERENCES legal_entities(id) ON DELETE RESTRICT,
    template_intent_id uuid REFERENCES tpl_org_intents(id),
    name varchar(300) NOT NULL,
    description text,
    sort_order int NOT NULL DEFAULT 0,
    status varchar(20) NOT NULL DEFAULT 'PLANNED',
    actual_start date,
    actual_end date,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_org_intents_legal_entity ON org_intents(legal_entity_id);

CREATE TABLE IF NOT EXISTS org_stages (
    id uuid PRIMARY KEY,
    intent_id uuid NOT NULL REFERENCES org_intents(id) ON DELETE RESTRICT,
    template_stage_id uuid REFERENCES tpl_org_stages(id),
    name varchar(300) NOT NULL,
    description text,
    sort_order int NOT NULL DEFAULT 0,
    status varchar(20) NOT NULL DEFAULT 'PLANNED',
    planned_start date,
    planned_end date,
    actual_start date,
    actual_end date,
    dependency_type varchar(10) NOT NULL DEFAULT 'FS',
    predecessor_stage_ids text,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_org_stages_intent ON org_stages(intent_id);

CREATE TABLE IF NOT EXISTS org_tasks (
    id uuid PRIMARY KEY,
    stage_id uuid NOT NULL REFERENCES org_stages(id) ON DELETE RESTRICT,
    template_offer_id uuid REFERENCES tpl_org_offers(id),
    name varchar(300) NOT NULL,
    description text,
    sort_order int NOT NULL DEFAULT 0,
    status varchar(20) NOT NULL DEFAULT 'PLANNED',
    assigned_user_id uuid REFERENCES users(id),
    assigned_role_id uuid REFERENCES ref_roles(id),
    assigned_board_role_id uuid REFERENCES ref_board_roles(id),
    candidate_roles text,
    predecessor_task_ids text,
    dependency_type varchar(10) NOT NULL DEFAULT 'FS',
    planned_start date,
    planned_end date,
    actual_start date,
    actual_end date,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_org_tasks_stage ON org_tasks(stage_id);
CREATE INDEX IF NOT EXISTS ix_org_tasks_user ON org_tasks(assigned_user_id);

-- org_milestones: реальные вехи (создаются из шаблонов tpl_org_milestones)
CREATE TABLE IF NOT EXISTS org_milestones (
    id uuid PRIMARY KEY,
    intent_id uuid NOT NULL REFERENCES org_intents(id) ON DELETE RESTRICT,
    template_milestone_id uuid REFERENCES tpl_org_milestones(id),
    stage_id uuid REFERENCES org_stages(id) ON DELETE RESTRICT,
    name varchar(300) NOT NULL,
    description text,
    milestone_type varchar(20) NOT NULL,
    predecessor_task_ids text,
    predecessor_stage_ids text,
    planned_date date,
    actual_date date,
    status varchar(20) NOT NULL DEFAULT 'PLANNED',
    sort_order int NOT NULL DEFAULT 0,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_org_milestones_intent ON org_milestones(intent_id);
CREATE INDEX IF NOT EXISTS ix_org_milestones_stage ON org_milestones(stage_id);

-- tpl_org_offer_roles: связь офер-роль (пул кандидатов)
CREATE TABLE IF NOT EXISTS tpl_org_offer_roles (
    id uuid PRIMARY KEY,
    tpl_offer_id uuid NOT NULL REFERENCES tpl_org_offers(id) ON DELETE RESTRICT,
    role_id uuid NOT NULL REFERENCES ref_roles(id) ON DELETE RESTRICT,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_tpl_offer_roles ON tpl_org_offer_roles(tpl_offer_id, role_id);
CREATE INDEX IF NOT EXISTS ix_tpl_offer_roles_role ON tpl_org_offer_roles(role_id);

-- ============================================================================
-- Junction-таблицы файлов (BDR-011: Единая таблица файлов)
-- Паттерн: {entity}_files — связь сущностей с таблицей files
-- file_type и display_name хранятся в таблице files
-- ============================================================================

-- meeting_files: файлы заседаний СД
CREATE TABLE IF NOT EXISTS meeting_files (
    id uuid PRIMARY KEY,
    meeting_id uuid NOT NULL REFERENCES meetings(id) ON DELETE RESTRICT,
    file_id uuid NOT NULL REFERENCES files(id) ON DELETE RESTRICT,
    CONSTRAINT ux_meeting_files_unique UNIQUE (meeting_id, file_id)
);
CREATE INDEX IF NOT EXISTS ix_meeting_files_meeting_id ON meeting_files(meeting_id);
CREATE INDEX IF NOT EXISTS ix_meeting_files_file_id ON meeting_files(file_id);

-- agenda_question_files: файлы вопросов повестки
CREATE TABLE IF NOT EXISTS agenda_question_files (
    id uuid PRIMARY KEY,
    agenda_question_id uuid NOT NULL REFERENCES agenda_questions(id) ON DELETE RESTRICT,
    file_id uuid NOT NULL REFERENCES files(id) ON DELETE RESTRICT,
    CONSTRAINT ux_agenda_question_files_unique UNIQUE (agenda_question_id, file_id)
);
CREATE INDEX IF NOT EXISTS ix_aqf_agenda_question_id ON agenda_question_files(agenda_question_id);
CREATE INDEX IF NOT EXISTS ix_aqf_file_id ON agenda_question_files(file_id);

-- committee_task_files: файлы задач комитетов
CREATE TABLE IF NOT EXISTS committee_task_files (
    id uuid PRIMARY KEY,
    committee_task_id uuid NOT NULL REFERENCES committee_tasks(id) ON DELETE RESTRICT,
    file_id uuid NOT NULL REFERENCES files(id) ON DELETE RESTRICT,
    CONSTRAINT ux_committee_task_files_unique UNIQUE (committee_task_id, file_id)
);
CREATE INDEX IF NOT EXISTS ix_ctf_committee_task_id ON committee_task_files(committee_task_id);
CREATE INDEX IF NOT EXISTS ix_ctf_file_id ON committee_task_files(file_id);

-- org_task_files: файлы задач оргплана
CREATE TABLE IF NOT EXISTS org_task_files (
    id uuid PRIMARY KEY,
    org_task_id uuid NOT NULL REFERENCES org_tasks(id) ON DELETE RESTRICT,
    file_id uuid NOT NULL REFERENCES files(id) ON DELETE RESTRICT,
    CONSTRAINT ux_org_task_files_unique UNIQUE (org_task_id, file_id)
);
CREATE INDEX IF NOT EXISTS ix_otf_org_task_id ON org_task_files(org_task_id);
CREATE INDEX IF NOT EXISTS ix_otf_file_id ON org_task_files(file_id);

-- committee_files: файлы комитетов
CREATE TABLE IF NOT EXISTS committee_files (
    id uuid PRIMARY KEY,
    committee_id uuid NOT NULL REFERENCES committees(id) ON DELETE RESTRICT,
    file_id uuid NOT NULL REFERENCES files(id) ON DELETE RESTRICT,
    CONSTRAINT ux_committee_files_unique UNIQUE (committee_id, file_id)
);
CREATE INDEX IF NOT EXISTS ix_cf_committee_id ON committee_files(committee_id);
CREATE INDEX IF NOT EXISTS ix_cf_file_id ON committee_files(file_id);

-- ============================================================================
-- Тестовые заседания TrueConf
-- ============================================================================

-- Таблица: trueconf_test_meeting (тестовое заседание СД через TrueConf)
CREATE TABLE IF NOT EXISTS trueconf_test_meeting (
    id uuid PRIMARY KEY,
    title varchar(200) NOT NULL,
    description text,
    trueconf_conference_id varchar(100),
    trueconf_join_link text,
    conference_state varchar(50),
    started_at timestamp with time zone,
    ended_at timestamp with time zone,
    all_members_voted boolean DEFAULT FALSE NOT NULL,
    decision_accepted boolean,
    status varchar(20) DEFAULT 'PREPARING' NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_ttmeeting_status ON trueconf_test_meeting(status);
CREATE INDEX IF NOT EXISTS ix_ttmeeting_conference_id ON trueconf_test_meeting(trueconf_conference_id);

-- Таблица: trueconf_test_question (вопросы тестового заседания)
CREATE TABLE IF NOT EXISTS trueconf_test_question (
    id uuid PRIMARY KEY,
    meeting_id uuid NOT NULL REFERENCES trueconf_test_meeting(id) ON DELETE RESTRICT,
    sequence_number int NOT NULL,
    question_text text NOT NULL,
    proposed_resolution text DEFAULT '',
    trueconf_poll_id varchar(100),
    poll_state varchar(20),
    status varchar(20) DEFAULT 'PENDING' NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_ttquestion_meeting_id ON trueconf_test_question(meeting_id);
CREATE INDEX IF NOT EXISTS ix_ttquestion_poll_id ON trueconf_test_question(trueconf_poll_id);

-- Таблица: trueconf_test_answer (ответы/голоса на вопросы тестового заседания)
CREATE TABLE IF NOT EXISTS trueconf_test_answer (
    id uuid PRIMARY KEY,
    question_id uuid NOT NULL REFERENCES trueconf_test_question(id) ON DELETE RESTRICT,
    user_name varchar(100) NOT NULL,
    vote_value varchar(20) NOT NULL,
    voted_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_ttanswer_question_id ON trueconf_test_answer(question_id);

-- Сотрудник (employee) — связывает ФЛ с ЮЛ и должностью
CREATE TABLE IF NOT EXISTS employee (
    id uuid PRIMARY KEY,
    person_id uuid NOT NULL REFERENCES persons(id) ON DELETE RESTRICT,
    legal_entity_id uuid NOT NULL REFERENCES legal_entities(id) ON DELETE RESTRICT,
    position varchar(200) NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid NOT NULL REFERENCES users(id)
);

CREATE INDEX IF NOT EXISTS ix_employee_person_id ON employee(person_id);
CREATE INDEX IF NOT EXISTS ix_employee_legal_entity_id ON employee(legal_entity_id);

-- system_settings: системные настройки (ключ-значение)
CREATE TABLE IF NOT EXISTS system_settings (
    id uuid PRIMARY KEY,
    key varchar(100) UNIQUE NOT NULL,
    value text,
    description text,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp with time zone
);

-- ============================================================================
-- Реестр участников общества (Board Portal)
-- Хранит актуальный состав участников с данными ДУЛ/реквизитов ЮЛ.
-- Источник: ручной ввод или импорт из СПАРК.
-- ============================================================================

CREATE TABLE IF NOT EXISTS board_participant (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    legal_entity_id uuid NOT NULL REFERENCES legal_entities(id) ON DELETE RESTRICT,
    participant_type varchar(20) NOT NULL DEFAULT 'FL',
    full_name varchar(300),
    passport_series varchar(10),
    passport_number varchar(10),
    passport_issued_by varchar(500),
    passport_issue_date date,
    passport_department_code varchar(10),
    passport_registration_address text,
    person_inn varchar(12),
    citizenship varchar(100),
    company_name varchar(500),
    company_inn varchar(12),
    company_ogrn varchar(15),
    company_kpp varchar(9),
    company_address text,
    ogrnip varchar(15),
    share_percent numeric(5,2),
    share_amount numeric(18,2),
    payment_info varchar(500),
    share_registration_info varchar(500),
    entry_date date,
    exit_date date,
    is_active boolean NOT NULL DEFAULT true,
    sort_order int NOT NULL DEFAULT 0,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid,
    person_id uuid REFERENCES persons(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS ix_board_participant_legal_entity ON board_participant(legal_entity_id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_board_participant_le_sort ON board_participant(legal_entity_id, sort_order);

-- ============================================================================
-- Доли, принадлежащие Обществу (казначейские доли)
-- ============================================================================

CREATE TABLE IF NOT EXISTS board_treasury_share (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    legal_entity_id uuid NOT NULL REFERENCES legal_entities(id) ON DELETE RESTRICT,
    share_percent numeric(5,2),
    share_amount numeric(18,2),
    acquired_date date,
    acquisition_basis varchar(500),
    sort_order int NOT NULL DEFAULT 0,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid
);

CREATE INDEX IF NOT EXISTS ix_board_treasury_share_legal_entity ON board_treasury_share(legal_entity_id);

-- ============================================================================
-- Акты загрузки реестра участников (XML + подпись)
-- ============================================================================

CREATE TABLE IF NOT EXISTS board_registry_upload (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    legal_entity_id uuid NOT NULL REFERENCES legal_entities(id) ON DELETE RESTRICT,
    xml_file_id uuid REFERENCES files(id) ON DELETE SET NULL,
    signature_file_id uuid REFERENCES files(id) ON DELETE SET NULL,
    xml_original_name varchar(255),
    signature_original_name varchar(255),
    status varchar(20) NOT NULL DEFAULT 'uploaded',
    participant_count int,
    uploaded_by uuid REFERENCES users(id) ON DELETE SET NULL,
    uploaded_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_board_registry_upload_le ON board_registry_upload(legal_entity_id);

-- ============================================================================
-- Информирование об изменении сведений участника
-- ============================================================================

CREATE TABLE IF NOT EXISTS board_participant_change (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    legal_entity_id uuid NOT NULL REFERENCES legal_entities(id) ON DELETE RESTRICT,
    participant_id uuid NOT NULL REFERENCES board_participant(id) ON DELETE RESTRICT,
    participant_type varchar(20) NOT NULL,
    -- ФЛ
    full_name varchar(300),
    passport_series varchar(10),
    passport_number varchar(10),
    passport_issued_by varchar(500),
    passport_issue_date date,
    passport_department_code varchar(10),
    passport_registration_address text,
    person_inn varchar(12),
    citizenship varchar(100),
    -- ЮЛ
    company_name varchar(500),
    company_inn varchar(12),
    company_ogrn varchar(15),
    company_kpp varchar(9),
    company_address text,
    -- ИП
    ogrnip varchar(15),
    -- Доля
    share_percent numeric(5,2),
    share_amount numeric(18,2),
    -- Мета
    document_file_id uuid REFERENCES files(id) ON DELETE SET NULL,
    document_original_name varchar(255),
    source varchar(20),
    date varchar(50),
    paper_doc_number varchar(100),
    comment text,
    submitted_by uuid REFERENCES users(id) ON DELETE SET NULL,
    submitted_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    status varchar(20) NOT NULL DEFAULT 'pending',
    review_comment text,
    reviewed_by uuid REFERENCES users(id) ON DELETE SET NULL,
    reviewed_at timestamp with time zone,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_board_participant_change_le ON board_participant_change(legal_entity_id);
CREATE INDEX IF NOT EXISTS ix_board_participant_change_participant ON board_participant_change(participant_id);

-- ============================================================================
-- Нотариальные заверения (notarization) — единая таблица
-- ============================================================================
CREATE TABLE IF NOT EXISTS notarization (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    legal_entity_id uuid NOT NULL REFERENCES legal_entities(id) ON DELETE RESTRICT,
    document_type varchar(50) NOT NULL,
    related_entity_id uuid,
    related_entity_type varchar(50),
    document_file_id uuid NOT NULL REFERENCES files(id) ON DELETE RESTRICT,
    notary_full_name varchar(300) NOT NULL,
    notary_license_number varchar(100),
    registry_number varchar(100),
    notarization_date date NOT NULL,
    valid_from date,
    valid_until date,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by uuid REFERENCES users(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS ix_notarization_le ON notarization(legal_entity_id);
CREATE INDEX IF NOT EXISTS ix_notarization_type ON notarization(document_type);
CREATE INDEX IF NOT EXISTS ix_notarization_related ON notarization(related_entity_type, related_entity_id);

-- ============================================================================
-- Запросы участника ООО в общество (share_request)
-- ============================================================================
CREATE TABLE IF NOT EXISTS share_request (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    legal_entity_id uuid NOT NULL REFERENCES legal_entities(id) ON DELETE RESTRICT,
    participant_id uuid NOT NULL REFERENCES board_participant(id) ON DELETE RESTRICT,
    request_type_id uuid NOT NULL REFERENCES ref_request_type(id) ON DELETE RESTRICT,
    status varchar(20) NOT NULL DEFAULT 'draft',
    payload jsonb,
    notarization_id uuid REFERENCES notarization(id) ON DELETE SET NULL,
    revoked_at timestamp with time zone,
    revoked_by_notarized boolean NOT NULL DEFAULT FALSE,
    visible_to_all boolean NOT NULL DEFAULT FALSE,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    completed_at timestamp with time zone,
    created_by uuid REFERENCES users(id) ON DELETE SET NULL,
    -- Коллективные требования
    is_collective boolean NOT NULL DEFAULT FALSE,
    threshold_percent numeric(4,2),
    total_support_percent numeric(6,2) NOT NULL DEFAULT 0,
    supporter_count integer NOT NULL DEFAULT 0,
    collective_status varchar(20),
    submitted_to_ceo_at timestamp with time zone,
    ceo_decision_at timestamp with time zone,
    ceo_comment text,
    decided_by_user_id uuid REFERENCES users(id) ON DELETE SET NULL,
    -- Место ознакомления (для способа "Ознакомление в офисе")
    review_location text,
    -- Орг-план ВОСУ
    org_intent_id uuid REFERENCES org_intents(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS ix_share_request_le ON share_request(legal_entity_id);
CREATE INDEX IF NOT EXISTS ix_share_request_participant ON share_request(participant_id);
CREATE INDEX IF NOT EXISTS ix_share_request_type_id ON share_request(request_type_id);
CREATE INDEX IF NOT EXISTS ix_share_request_status ON share_request(status);
CREATE INDEX IF NOT EXISTS ix_share_request_visible ON share_request(visible_to_all) WHERE visible_to_all = TRUE;

-- ============================================================
-- SHARE REQUEST SUPPORT — поддержки коллективных требований
-- ============================================================

CREATE TABLE IF NOT EXISTS share_request_support (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    share_request_id uuid NOT NULL REFERENCES share_request(id) ON DELETE RESTRICT,
    participant_id uuid NOT NULL REFERENCES board_participant(id) ON DELETE RESTRICT,
    share_percent_at_support numeric(6,2) NOT NULL,
    supported_at timestamp with time zone NOT NULL DEFAULT NOW(),
    withdrawn_at timestamp with time zone,
    created_at timestamp with time zone NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_srs_request ON share_request_support(share_request_id);
CREATE INDEX IF NOT EXISTS ix_srs_participant ON share_request_support(participant_id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_srs_request_participant ON share_request_support(share_request_id, participant_id) WHERE withdrawn_at IS NULL;

-- ============================================================
-- SHARE REQUEST FILES — файлы требований участников
-- ============================================================

CREATE TABLE IF NOT EXISTS share_request_files (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    share_request_id uuid NOT NULL REFERENCES share_request(id) ON DELETE RESTRICT,
    file_id uuid NOT NULL REFERENCES files(id) ON DELETE RESTRICT,
    CONSTRAINT ux_share_request_files UNIQUE (share_request_id, file_id)
);
CREATE INDEX IF NOT EXISTS ix_srf_request ON share_request_files(share_request_id);
CREATE INDEX IF NOT EXISTS ix_srf_file ON share_request_files(file_id);

-- ============================================================
-- REF_DOCUMENT_TYPE — справочник типов документов для требования
-- ============================================================

CREATE TABLE IF NOT EXISTS ref_document_type (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    code varchar(50) NOT NULL,
    name varchar(300) NOT NULL,
    group_code varchar(50) NOT NULL,
    group_name varchar(200) NOT NULL,
    is_electronic_available boolean NOT NULL DEFAULT FALSE,
    is_unitary boolean NOT NULL DEFAULT FALSE,
    storage_years integer NOT NULL DEFAULT 3,
    is_for_llc boolean NOT NULL DEFAULT FALSE,
    is_for_njsc boolean NOT NULL DEFAULT FALSE,
    is_for_pjsc boolean NOT NULL DEFAULT FALSE,
    sort_order integer DEFAULT 0,
    created_at timestamp with time zone NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_ref_document_type_code ON ref_document_type(code);

-- ============================================================
-- REF_DOCUMENT_ACCESS_METHOD — справочник способов доступа к документам
-- ============================================================

CREATE TABLE IF NOT EXISTS ref_document_access_method (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    code varchar(50) NOT NULL,
    name varchar(300) NOT NULL,
    description text,
    deadline_days integer,
    sort_order integer DEFAULT 0,
    created_at timestamp with time zone NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_ref_access_method_code ON ref_document_access_method(code);

-- ============================================================
-- REF_DOCUMENT_REFUSAL_REASON — справочник причин отказа
-- ============================================================

CREATE TABLE IF NOT EXISTS ref_document_refusal_reason (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    code varchar(50) NOT NULL,
    name varchar(300) NOT NULL,
    description text,
    legal_basis varchar(300),
    sort_order integer DEFAULT 0,
    created_at timestamp with time zone NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_ref_refusal_reason_code ON ref_document_refusal_reason(code);
