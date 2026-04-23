-- Schemas
create schema if not exists system;
create schema if not exists learning;
create schema if not exists simulation;
create schema if not exists module;
create schema if not exists public;

-- Extensions
create extension if not exists "pgcrypto";

-- ============================================================
-- SYSTEM SCHEMA
-- ============================================================

create table if not exists system.audit_logs
(
    audit_log_guid  uuid        primary key default gen_random_uuid(),
    table_name      text        not null,
    record_guid     uuid        not null,
    action          smallint    not null,
    old_values      jsonb,
    new_values      jsonb,
    changed_fields  text[],
    fk_user_guid    uuid,
    ip_address      inet,
    device_id       uuid,
    created_at      timestamptz not null default now()
    );

create index if not exists idx_audit_logs_record_guid
    on system.audit_logs(record_guid);

create table if not exists public.institutes
(
    institute_guid          uuid        primary key default gen_random_uuid(),
    institute_id            int         generated always as identity unique,
    name                    text        not null,
    branch                  text        not null,
    created_at              timestamptz not null default now(),
    created_by_user_guid    uuid,
    updated_at              timestamptz,
    updated_by_user_guid    uuid,
    deleted_at              timestamptz,
    deleted_by_user_guid    uuid
    );

create table if not exists public.users
(
    user_guid               uuid        primary key default gen_random_uuid(),
    user_id                 int         generated always as identity unique,
    email                   text        not null unique,
    password_hashed         text,
    username                text        not null unique,
    first_name              text        not null,
    last_name               text        not null,
    role_id                 smallint    not null,
    login_attempts          smallint    not null default 0,
    is_blocked              boolean     not null default false,
    state                   smallint    not null,
    fk_institute_guid       uuid        not null,
    created_at              timestamptz not null default now(),
    created_by_user_guid    uuid,
    updated_at              timestamptz,
    updated_by_user_guid    uuid,
    deleted_at              timestamptz,
    deleted_by_user_guid    uuid,

    constraint fk_users_institute
    foreign key (fk_institute_guid)
    references public.institutes(institute_guid)
    on delete restrict,

    constraint fk_users_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_users_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_users_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_users_institute_guid
    on public.users(fk_institute_guid);

create table if not exists system.user_settings
(
    user_setting_guid   uuid        primary key default gen_random_uuid(),
    user_setting_id     int         generated always as identity unique,
    language            smallint    not null,
    dark_mode           boolean     not null default false,
    fk_user_guid		uuid 		not null,

    constraint fk_user_settings_users
    foreign key (fk_user_guid)
    references public.users(user_guid)
    on delete cascade
    );

create index if not exists idx_user_settings_users
    on system.user_settings(fk_user_guid);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_institutes_created_by'
    ) THEN
ALTER TABLE public.institutes
    ADD CONSTRAINT fk_institutes_created_by
        FOREIGN KEY (created_by_user_guid)
            REFERENCES public.users(user_guid)
            ON DELETE restrict;
END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_institutes_updated_by'
    ) THEN
ALTER TABLE public.institutes
    ADD CONSTRAINT fk_institutes_updated_by
        FOREIGN KEY (updated_by_user_guid)
            REFERENCES public.users(user_guid)
            ON DELETE restrict;
END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_institutes_deleted_by'
    ) THEN
ALTER TABLE public.institutes
    ADD CONSTRAINT fk_institutes_deleted_by
        FOREIGN KEY (deleted_by_user_guid)
            REFERENCES public.users(user_guid)
            ON DELETE restrict;
END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_audit_logs_user'
    ) THEN
ALTER TABLE system.audit_logs
    ADD CONSTRAINT fk_audit_logs_user
        FOREIGN KEY (fk_user_guid)
            REFERENCES public.users(user_guid)
            ON DELETE set null;
END IF;
END
$$;

create index if not exists idx_institutes_created_by
    on public.institutes(created_by_user_guid);

create index if not exists idx_institutes_updated_by
    on public.institutes(updated_by_user_guid);

create index if not exists idx_institutes_deleted_by
    on public.institutes(deleted_by_user_guid);

create table if not exists system.institute_settings
(
    institute_setting_guid      uuid        primary key default gen_random_uuid(),
    institute_setting_id        int         generated always as identity unique,
    four_eyes_approach          boolean     not null default false,
    updated_at                  timestamptz,
    updated_by_user_guid        uuid,
    fk_institute_guid			uuid not null,

    constraint fk_institute_settings_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete set null,

    constraint fk_institute_settings_institutes
    foreign key (fk_institute_guid)
    references public.institutes(institute_guid)
    on delete cascade
    );

create index if not exists idx_institute_settings_updated_by
    on system.institute_settings(updated_by_user_guid);

create index if not exists idx_institute_settings_institutes_guid
    on system.institute_settings(fk_institute_guid);

create table if not exists system.refresh_tokens
(
    refresh_token_guid  uuid        primary key default gen_random_uuid(),
    refresh_token       uuid        not null default gen_random_uuid(),
    expiration_date     timestamptz not null,
    device_id           uuid        not null,
    device_name         text        not null,
    ip_address          inet        not null,
    revoked             boolean     not null default false,
    revoked_date        timestamptz,
    created_date        timestamptz not null default now(),
    last_used_date      timestamptz not null default now(),
    fk_user_guid        uuid        not null,

    constraint fk_refresh_tokens_user
    foreign key (fk_user_guid)
    references public.users(user_guid)
    on delete cascade
    );

create index if not exists idx_refresh_tokens_user_guid
    on system.refresh_tokens(fk_user_guid);

create index if not exists idx_refresh_tokens_token
    on system.refresh_tokens(refresh_token);

create table if not exists system.prompts
(
    prompt_guid             uuid        primary key default gen_random_uuid(),
    prompt_id               int         generated always as identity unique,
    description             text        not null,
    model                   text        not null,
    version                 text        not null,
    prompt_type             text        not null,
    prompt_system           text        not null,
    prompt_task             text        not null,
    prompt_example          text        not null,
    prompt_example_answer   text        not null,
    prompt_temperature      numeric     not null,
    prompt_language         text        not null,
    max_tokens              int         not null,
    created_at              timestamptz not null default now(),
    created_by_user_guid    uuid,
    updated_at              timestamptz,
    updated_by_user_guid    uuid,
    deleted_at              timestamptz,
    deleted_by_user_guid    uuid,

    constraint fk_prompts_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_prompts_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_prompts_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_prompts_created_by
    on system.prompts(created_by_user_guid);

create index if not exists idx_prompts_updated_by
    on system.prompts(updated_by_user_guid);

create index if not exists idx_prompts_deleted_by
    on system.prompts(deleted_by_user_guid);

-- ============================================================
-- LEARNING SCHEMA
-- ============================================================

create table if not exists learning.courses
(
    course_guid             uuid        primary key default gen_random_uuid(),
    course_id               int         generated always as identity unique,
    title                   text        not null,
    description             text        not null,
    state                   smallint    not null,
    created_at              timestamptz not null default now(),
    created_by_user_guid    uuid,
    updated_at              timestamptz,
    updated_by_user_guid    uuid,
    deleted_at              timestamptz,
    deleted_by_user_guid    uuid,

    constraint fk_courses_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_courses_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_courses_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_courses_created_by
    on learning.courses(created_by_user_guid);

create index if not exists idx_courses_updated_by
    on learning.courses(updated_by_user_guid);

create index if not exists idx_courses_deleted_by
    on learning.courses(deleted_by_user_guid);

create table if not exists learning.course_responsibilities
(
    course_responsibility_guid  uuid    primary key default gen_random_uuid(),
    fk_course_guid              uuid    not null,
    fk_teacher_user_guid        uuid    not null,

    constraint fk_course_responsibilities_course
    foreign key (fk_course_guid)
    references learning.courses(course_guid)
    on delete cascade,

    constraint fk_course_responsibilities_user
    foreign key (fk_teacher_user_guid)
    references public.users(user_guid)
    on delete cascade,

    constraint uq_course_responsibilities_course_teacher
    unique (fk_course_guid, fk_teacher_user_guid)
    );

create index if not exists idx_course_responsibilities_course_guid
    on learning.course_responsibilities(fk_course_guid);

create index if not exists idx_course_responsibilities_teacher_user_guid
    on learning.course_responsibilities(fk_teacher_user_guid);

create table if not exists learning.units
(
    unit_guid               uuid        primary key default gen_random_uuid(),
    unit_id                 int         generated always as identity unique,
    title                   text        not null,
    description             text        not null,
    topic                   text        not null,
    state                   smallint    not null,
    fk_course_guid          uuid        not null,
    created_at              timestamptz not null default now(),
    created_by_user_guid    uuid,
    updated_at              timestamptz,
    updated_by_user_guid    uuid,
    deleted_at              timestamptz,
    deleted_by_user_guid    uuid,

    constraint fk_units_course
    foreign key (fk_course_guid)
    references learning.courses(course_guid)
    on delete cascade,

    constraint fk_units_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_units_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_units_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_units_created_by
    on learning.units(created_by_user_guid);

create index if not exists idx_units_updated_by
    on learning.units(updated_by_user_guid);

create index if not exists idx_units_deleted_by
    on learning.units(deleted_by_user_guid);

create index if not exists idx_units_course_guid
    on learning.units(fk_course_guid);

create table if not exists learning.unit_approvals
(
    unit_approval_guid              uuid    primary key default gen_random_uuid(),
    fk_unit_guid                    uuid    not null,
    fk_course_responsibility_guid   uuid    not null,

    constraint fk_unit_approvals_unit
    foreign key (fk_unit_guid)
    references learning.units(unit_guid)
    on delete cascade,

    constraint fk_unit_approvals_course_responsibility
    foreign key (fk_course_responsibility_guid)
    references learning.course_responsibilities(course_responsibility_guid)
    on delete cascade
    );

create index if not exists idx_unit_approvals_unit_guid
    on learning.unit_approvals(fk_unit_guid);

create index if not exists idx_unit_approvals_course_responsibility_guid
    on learning.unit_approvals(fk_course_responsibility_guid);

create table if not exists learning.tasks
(
    task_guid               uuid        primary key default gen_random_uuid(),
    task_id                 int         generated always as identity unique,
    title                   text        not null,
    description             text        not null,
    type                    smallint    not null,
    created_at              timestamptz not null default now(),
    created_by_user_guid    uuid,
    updated_at              timestamptz,
    updated_by_user_guid    uuid,
    deleted_at              timestamptz,
    deleted_by_user_guid    uuid,

    constraint fk_tasks_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_tasks_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_tasks_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_tasks_created_by
    on learning.tasks(created_by_user_guid);

create index if not exists idx_tasks_updated_by
    on learning.tasks(updated_by_user_guid);

create index if not exists idx_tasks_deleted_by
    on learning.tasks(deleted_by_user_guid);

create table if not exists learning.learning_materials
(
    learning_material_guid  uuid    primary key default gen_random_uuid(),
    learning_material_id    int     generated always as identity unique,
    title                   text    not null,
    blob_storage_key        text    not null,
    fk_task_guid            uuid    not null,

    constraint fk_learning_materials_task
    foreign key (fk_task_guid)
    references learning.tasks(task_guid)
    on delete cascade
    );

create index if not exists idx_learning_materials_task_guid
    on learning.learning_materials(fk_task_guid);

create table if not exists learning.learning_material_students
(
    learning_material_student_guid  uuid        primary key default gen_random_uuid(),
    completion_date                 timestamptz not null,
    fk_learning_material_guid       uuid        not null,
    fk_student_guid                 uuid        not null,

    constraint fk_learning_material_students_learning_material
    foreign key (fk_learning_material_guid)
    references learning.learning_materials(learning_material_guid)
    on delete cascade,

    constraint fk_learning_material_students_user
    foreign key (fk_student_guid)
    references public.users(user_guid)
    on delete cascade
    );

create index if not exists idx_learning_material_students_learning_material_guid
    on learning.learning_material_students(fk_learning_material_guid);

create index if not exists idx_learning_material_students_student_guid
    on learning.learning_material_students(fk_student_guid);

-- ============================================================
-- SIMULATION SCHEMA
-- ============================================================

create table if not exists simulation.simulation_setups
(
    simulation_setup_guid   uuid    primary key default gen_random_uuid(),
    simulation_setup_id     int     generated always as identity unique,
    fk_prompt_guid          uuid    not null,

    constraint fk_simulation_setups_prompt
    foreign key (fk_prompt_guid)
    references "system".prompts(prompt_guid)
    on delete restrict
    );

create index if not exists idx_simulation_setups_prompt_guid
    on simulation.simulation_setups(fk_prompt_guid);


create table if not exists simulation.simulations
(
    simulation_guid             uuid        primary key default gen_random_uuid(),
    simulation_id               int         generated always as identity unique,
    name                        text        not null,
    fk_task_guid                uuid        not null,
    fk_simulation_setup_guid    uuid        not null,
    created_at                  timestamptz not null default now(),
    created_by_user_guid        uuid,
    updated_at                  timestamptz,
    updated_by_user_guid        uuid,
    deleted_at                  timestamptz,
    deleted_by_user_guid        uuid,

    constraint fk_simulations_task
    foreign key (fk_task_guid)
    references learning.tasks(task_guid)
    on delete cascade,

    constraint fk_simulations_simulation_setup
    foreign key (fk_simulation_setup_guid)
    references simulation.simulation_setups(simulation_setup_guid)
    on delete restrict,

    constraint fk_simulations_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulations_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulations_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_simulations_created_by
    on simulation.simulations(created_by_user_guid);

create index if not exists idx_simulations_updated_by
    on simulation.simulations(updated_by_user_guid);

create index if not exists idx_simulations_deleted_by
    on simulation.simulations(deleted_by_user_guid);

create index if not exists idx_simulations_task_guid
    on simulation.simulations(fk_task_guid);

create index if not exists idx_simulations_simulation_setup_guid
    on simulation.simulations(fk_simulation_setup_guid);

create table if not exists simulation.simulation_students
(
    simulation_student_guid uuid        primary key default gen_random_uuid(),
    completion_date         timestamptz not null,
    is_passed               boolean     not null,
    fk_simulation_guid      uuid        not null,
    fk_student_guid         uuid        not null,

    constraint fk_simulation_students_simulation
    foreign key (fk_simulation_guid)
    references simulation.simulations(simulation_guid)
    on delete cascade,

    constraint fk_simulation_students_user
    foreign key (fk_student_guid)
    references public.users(user_guid)
    on delete cascade,

    constraint uq_simulation_students_simulation_student
    unique (fk_simulation_guid, fk_student_guid)
    );

create index if not exists idx_simulation_students_simulation_guid
    on simulation.simulation_students(fk_simulation_guid);

create index if not exists idx_simulation_students_student_guid
    on simulation.simulation_students(fk_student_guid);

create table if not exists simulation.simulation_feedbacks
(
    simulation_feedback_guid        uuid        primary key default gen_random_uuid(),
    simulation_feedback_id          int         generated always as identity unique,
    feedback_text                   text        not null,
    rating                          smallint    not null,
    created_at                      timestamptz not null default now(),
    fk_simulation_student_guid      uuid        not null,

    constraint fk_simulation_feedbacks_simulation_student
    foreign key (fk_simulation_student_guid)
    references simulation.simulation_students(simulation_student_guid)
    on delete cascade
    );

create index if not exists idx_simulation_feedbacks_simulation_student_guid
    on simulation.simulation_feedbacks(fk_simulation_student_guid);

create table if not exists simulation.simulation_checklists
(
    simulation_checklist_guid   uuid    primary key default gen_random_uuid(),
    simulation_checklist_id     int     generated always as identity unique,
    fk_simulation_setup_guid    uuid,
    fk_prompt_guid              uuid,

    constraint fk_simulation_checklists_simulation_setup
    foreign key (fk_simulation_setup_guid)
    references simulation.simulation_setups(simulation_setup_guid)
    on delete set null,

    constraint fk_simulation_checklists_prompt
    foreign key (fk_prompt_guid)
    references "system".prompts(prompt_guid)
    on delete set null
    );

create index if not exists idx_simulation_checklists_simulation_setup_guid
    on simulation.simulation_checklists(fk_simulation_setup_guid);

create index if not exists idx_simulation_checklists_prompt_guid
    on simulation.simulation_checklists(fk_prompt_guid);

create table if not exists simulation.simulation_checklist_items
(
    simulation_checklist_item_guid  uuid        primary key default gen_random_uuid(),
    item                            text        not null,
    state                           smallint    not null,
    fk_simulation_checklist_guid    uuid        not null,
    created_at                      timestamptz not null default now(),
    created_by_user_guid            uuid,
    updated_at                      timestamptz,
    updated_by_user_guid            uuid,
    deleted_at                      timestamptz,
    deleted_by_user_guid            uuid,

    constraint fk_simulation_checklist_items_simulation_checklist
    foreign key (fk_simulation_checklist_guid)
    references simulation.simulation_checklists(simulation_checklist_guid)
    on delete cascade,

    constraint fk_simulation_checklist_items_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulation_checklist_items_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulation_checklist_items_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_simulation_checklist_items_created_by
    on simulation.simulation_checklist_items(created_by_user_guid);

create index if not exists idx_simulation_checklist_items_updated_by
    on simulation.simulation_checklist_items(updated_by_user_guid);

create index if not exists idx_simulation_checklist_items_deleted_by
    on simulation.simulation_checklist_items(deleted_by_user_guid);

create index if not exists idx_simulation_checklist_items_simulation_checklist_guid
    on simulation.simulation_checklist_items(fk_simulation_checklist_guid);

create table if not exists simulation.simulation_setup_images
(
    simulation_setup_image_guid uuid    primary key default gen_random_uuid(),
    simulation_setup_image_id   int     generated always as identity unique,
    blob_storage_key            text    not null,
    fk_simulation_setup_guid    uuid,

    constraint fk_simulation_setup_images_simulation_setup
    foreign key (fk_simulation_setup_guid)
    references simulation.simulation_setups(simulation_setup_guid)
    on delete set null
    );

create index if not exists idx_simulation_setup_images_simulation_setup_guid
    on simulation.simulation_setup_images(fk_simulation_setup_guid);

create table if not exists simulation.simulation_patient_datas
(
    simulation_patient_data_guid    uuid        primary key default gen_random_uuid(),
    simulation_patient_data_id      int         generated always as identity unique,
    name                            text        not null,
    year_of_birth                   smallint    not null,
    health_insurance                text        not null,
    profession                      text        not null,
    employer                        text        not null,
    fk_simulation_setup_guid        uuid,

    constraint fk_simulation_patient_datas_simulation_setup
    foreign key (fk_simulation_setup_guid)
    references simulation.simulation_setups(simulation_setup_guid)
    on delete set null
    );

create index if not exists idx_simulation_patient_datas_simulation_setup_guid
    on simulation.simulation_patient_datas(fk_simulation_setup_guid);

create table if not exists simulation.simulation_patient_data_allergies
(
    simulation_patient_data_allergy_guid    uuid        primary key default gen_random_uuid(),
    simulation_patient_data_allergy_id      int         generated always as identity unique,
    name                                    text        not null,
    fk_simulation_patient_data_guid         uuid        not null,
    created_at                              timestamptz not null default now(),
    created_by_user_guid                    uuid,
    updated_at                              timestamptz,
    updated_by_user_guid                    uuid,
    deleted_at                              timestamptz,
    deleted_by_user_guid                    uuid,

    constraint fk_simulation_patient_data_allergies_patient_data
    foreign key (fk_simulation_patient_data_guid)
    references simulation.simulation_patient_datas(simulation_patient_data_guid)
    on delete cascade,

    constraint fk_simulation_patient_data_allergies_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulation_patient_data_allergies_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulation_patient_data_allergies_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_simulation_patient_data_allergies_created_by
    on simulation.simulation_patient_data_allergies(created_by_user_guid);

create index if not exists idx_simulation_patient_data_allergies_updated_by
    on simulation.simulation_patient_data_allergies(updated_by_user_guid);

create index if not exists idx_simulation_patient_data_allergies_deleted_by
    on simulation.simulation_patient_data_allergies(deleted_by_user_guid);

create index if not exists idx_simulation_patient_data_allergies_patient_data_guid
    on simulation.simulation_patient_data_allergies(fk_simulation_patient_data_guid);

create table if not exists simulation.simulation_patient_data_current_ailments
(
    simulation_patient_data_current_ailment_guid    uuid        primary key default gen_random_uuid(),
    simulation_patient_data_current_ailment_id      int         generated always as identity unique,
    name                                            text        not null,
    fk_simulation_patient_data_guid                 uuid        not null,
    created_at                                      timestamptz not null default now(),
    created_by_user_guid                            uuid,
    updated_at                                      timestamptz,
    updated_by_user_guid                            uuid,
    deleted_at                                      timestamptz,
    deleted_by_user_guid                            uuid,

    constraint fk_simulation_patient_data_current_ailments_patient_data
    foreign key (fk_simulation_patient_data_guid)
    references simulation.simulation_patient_datas(simulation_patient_data_guid)
    on delete cascade,

    constraint fk_simulation_patient_data_current_ailments_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulation_patient_data_current_ailments_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulation_patient_data_current_ailments_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_simulation_patient_data_current_ailments_created_by
    on simulation.simulation_patient_data_current_ailments(created_by_user_guid);

create index if not exists idx_simulation_patient_data_current_ailments_updated_by
    on simulation.simulation_patient_data_current_ailments(updated_by_user_guid);

create index if not exists idx_simulation_patient_data_current_ailments_deleted_by
    on simulation.simulation_patient_data_current_ailments(deleted_by_user_guid);

create index if not exists idx_simulation_patient_data_current_ailments_patient_data_guid
    on simulation.simulation_patient_data_current_ailments(fk_simulation_patient_data_guid);

create table if not exists simulation.simulation_patient_data_diagnostics
(
    simulation_patient_data_diagnostic_guid  uuid        primary key default gen_random_uuid(),
    simulation_patient_data_diagnostic_id    int         generated always as identity unique,
    name                                     text        not null,
    fk_simulation_patient_data_guid          uuid        not null,
    created_at                               timestamptz not null default now(),
    created_by_user_guid                     uuid,
    updated_at                               timestamptz,
    updated_by_user_guid                     uuid,
    deleted_at                               timestamptz,
    deleted_by_user_guid                     uuid,

    constraint fk_simulation_patient_data_diagnostics_patient_data
    foreign key (fk_simulation_patient_data_guid)
    references simulation.simulation_patient_datas(simulation_patient_data_guid)
    on delete cascade,

    constraint fk_simulation_patient_data_diagnostics_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulation_patient_data_diagnostics_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulation_patient_data_diagnostics_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_simulation_patient_data_diagnostics_created_by
    on simulation.simulation_patient_data_diagnostics(created_by_user_guid);

create index if not exists idx_simulation_patient_data_diagnostics_updated_by
    on simulation.simulation_patient_data_diagnostics(updated_by_user_guid);

create index if not exists idx_simulation_patient_data_diagnostics_deleted_by
    on simulation.simulation_patient_data_diagnostics(deleted_by_user_guid);

create index if not exists idx_simulation_patient_data_diagnostics_patient_data_guid
    on simulation.simulation_patient_data_diagnostics(fk_simulation_patient_data_guid);

create table if not exists simulation.simulation_patient_data_restrictions
(
    simulation_patient_data_restriction_guid uuid        primary key default gen_random_uuid(),
    simulation_patient_data_restriction_id   int         generated always as identity unique,
    name                                     text        not null,
    fk_simulation_patient_data_guid          uuid        not null,
    created_at                               timestamptz not null default now(),
    created_by_user_guid                     uuid,
    updated_at                               timestamptz,
    updated_by_user_guid                     uuid,
    deleted_at                               timestamptz,
    deleted_by_user_guid                     uuid,

    constraint fk_simulation_patient_data_restrictions_patient_data
    foreign key (fk_simulation_patient_data_guid)
    references simulation.simulation_patient_datas(simulation_patient_data_guid)
    on delete cascade,

    constraint fk_simulation_patient_data_restrictions_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulation_patient_data_restrictions_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulation_patient_data_restrictions_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_simulation_patient_data_restrictions_created_by
    on simulation.simulation_patient_data_restrictions(created_by_user_guid);

create index if not exists idx_simulation_patient_data_restrictions_updated_by
    on simulation.simulation_patient_data_restrictions(updated_by_user_guid);

create index if not exists idx_simulation_patient_data_restrictions_deleted_by
    on simulation.simulation_patient_data_restrictions(deleted_by_user_guid);

create index if not exists idx_simulation_patient_data_restrictions_patient_data_guid
    on simulation.simulation_patient_data_restrictions(fk_simulation_patient_data_guid);

create table if not exists simulation.simulation_patient_data_social_status
(
    simulation_patient_data_social_status_guid   uuid        primary key default gen_random_uuid(),
    simulation_patient_data_social_status_id      int         generated always as identity unique,
    name                                          text        not null,
    fk_simulation_patient_data_guid               uuid        not null,
    created_at                                    timestamptz not null default now(),
    created_by_user_guid                          uuid,
    updated_at                                    timestamptz,
    updated_by_user_guid                          uuid,
    deleted_at                                    timestamptz,
    deleted_by_user_guid                          uuid,

    constraint fk_simulation_patient_data_social_status_patient_data
    foreign key (fk_simulation_patient_data_guid)
    references simulation.simulation_patient_datas(simulation_patient_data_guid)
    on delete cascade,

    constraint fk_simulation_patient_data_social_status_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulation_patient_data_social_status_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_simulation_patient_data_social_status_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_simulation_patient_data_social_status_created_by
    on simulation.simulation_patient_data_social_status(created_by_user_guid);

create index if not exists idx_simulation_patient_data_social_status_updated_by
    on simulation.simulation_patient_data_social_status(updated_by_user_guid);

create index if not exists idx_simulation_patient_data_social_status_deleted_by
    on simulation.simulation_patient_data_social_status(deleted_by_user_guid);

create index if not exists idx_simulation_patient_data_social_status_patient_data_guid
    on simulation.simulation_patient_data_social_status(fk_simulation_patient_data_guid);

-- ============================================================
-- MODULE SCHEMA
-- ============================================================

create table if not exists module.modules
(
    module_guid             uuid        primary key default gen_random_uuid(),
    module_id               int         generated always as identity unique,
    title                   text        not null,
    description             text        not null,
    created_at              timestamptz not null default now(),
    created_by_user_guid    uuid,
    updated_at              timestamptz,
    updated_by_user_guid    uuid,
    deleted_at              timestamptz,
    deleted_by_user_guid    uuid,

    constraint fk_modules_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_modules_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_modules_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_modules_created_by
    on module.modules(created_by_user_guid);

create index if not exists idx_modules_updated_by
    on module.modules(updated_by_user_guid);

create index if not exists idx_modules_deleted_by
    on module.modules(deleted_by_user_guid);

create table if not exists module.module_institutes
(
    module_institute_guid   uuid    primary key default gen_random_uuid(),
    fk_module_guid          uuid    not null,
    fk_institute_guid       uuid    not null,

    constraint fk_module_institutes_module
    foreign key (fk_module_guid)
    references module.modules(module_guid)
    on delete cascade,

    constraint fk_module_institutes_institute
    foreign key (fk_institute_guid)
    references public.institutes(institute_guid)
    on delete cascade,

    constraint uq_module_institutes_module_institute
    unique (fk_module_guid, fk_institute_guid)
    );

create index if not exists idx_module_institutes_module_guid
    on module.module_institutes(fk_module_guid);

create index if not exists idx_module_institutes_institute_guid
    on module.module_institutes(fk_institute_guid);

create table if not exists module.module_simulations
(
    module_simulation_guid  uuid        primary key default gen_random_uuid(),
    sort_order              smallint    not null,
    fk_module_guid          uuid        not null,
    fk_simulation_guid      uuid        not null,

    constraint fk_module_simulations_module
    foreign key (fk_module_guid)
    references module.modules(module_guid)
    on delete cascade,

    constraint fk_module_simulations_simulation
    foreign key (fk_simulation_guid)
    references simulation.simulations(simulation_guid)
    on delete cascade
    );

create index if not exists idx_module_simulations_module_guid
    on module.module_simulations(fk_module_guid);

create index if not exists idx_module_simulations_simulation_guid
    on module.module_simulations(fk_simulation_guid);

create table if not exists module.module_room_701_sessions
(
    module_room_701_session_guid    uuid        primary key default gen_random_uuid(),
    title                           text        not null,
    description                     text        not null,
    fk_module_guid                  uuid,
    fk_prompt_guid                  uuid        not null,
    created_at                      timestamptz not null default now(),
    created_by_user_guid            uuid,
    updated_at                      timestamptz,
    updated_by_user_guid            uuid,
    deleted_at                      timestamptz,
    deleted_by_user_guid            uuid,

    constraint fk_module_room_701_sessions_module
    foreign key (fk_module_guid)
    references module.modules(module_guid)
    on delete restrict,

    constraint fk_module_room_701_sessions_prompt
    foreign key (fk_prompt_guid)
    references "system".prompts(prompt_guid)
    on delete restrict,

    constraint fk_module_room_701_sessions_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_module_room_701_sessions_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_module_room_701_sessions_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_module_room_701_sessions_created_by
    on module.module_room_701_sessions(created_by_user_guid);

create index if not exists idx_module_room_701_sessions_updated_by
    on module.module_room_701_sessions(updated_by_user_guid);

create index if not exists idx_module_room_701_sessions_deleted_by
    on module.module_room_701_sessions(deleted_by_user_guid);

create index if not exists idx_module_room_701_sessions_module_guid
    on module.module_room_701_sessions(fk_module_guid);

create index if not exists idx_module_room_701_sessions_prompt_guid
    on module.module_room_701_sessions(fk_prompt_guid);

create table if not exists module.module_room_701_session_user_messages
(
    module_room_701_user_message_guid   uuid        primary key default gen_random_uuid(),
    message                             text        not null,
    sort_order                          smallint,
    fk_module_room_701_session_guid     uuid        not null,
    fk_user_guid                        uuid        not null,

    constraint fk_module_room_701_session_user_messages_session
    foreign key (fk_module_room_701_session_guid)
    references module.module_room_701_sessions(module_room_701_session_guid)
    on delete cascade,

    constraint fk_module_room_701_session_user_messages_user
    foreign key (fk_user_guid)
    references public.users(user_guid)
    on delete cascade
    );

create index if not exists idx_module_room_701_session_user_messages_session_guid
    on module.module_room_701_session_user_messages(fk_module_room_701_session_guid);

create index if not exists idx_module_room_701_session_user_messages_user_guid
    on module.module_room_701_session_user_messages(fk_user_guid);

create table if not exists module.module_room_701_session_model_messages
(
    module_room_701_model_message_guid  uuid        primary key default gen_random_uuid(),
    message                             text        not null,
    sort_order                          smallint,
    fk_module_room_701_session_guid     uuid        not null,
    fk_prompt_guid                      uuid        not null,

    constraint fk_module_room_701_session_model_messages_session
    foreign key (fk_module_room_701_session_guid)
    references module.module_room_701_sessions(module_room_701_session_guid)
    on delete cascade,

    constraint fk_module_room_701_session_model_messages_prompt
    foreign key (fk_prompt_guid)
    references "system".prompts(prompt_guid)
    on delete cascade
    );

create index if not exists idx_module_room_701_session_model_messages_session_guid
    on module.module_room_701_session_model_messages(fk_module_room_701_session_guid);

create index if not exists idx_module_room_701_session_model_messages_prompt_guid
    on module.module_room_701_session_model_messages(fk_prompt_guid);

create table if not exists module.module_fault_finding_questions
(
    fault_finding_question_guid uuid        primary key default gen_random_uuid(),
    fault_finding_question_id   int         generated always as identity unique,
    fault_finding_task          text        not null,
    fk_module_guid              uuid,
    created_at                  timestamptz not null default now(),
    created_by_user_guid        uuid,
    updated_at                  timestamptz,
    updated_by_user_guid        uuid,
    deleted_at                  timestamptz,
    deleted_by_user_guid        uuid,

    constraint fk_module_fault_finding_questions_module
    foreign key (fk_module_guid)
    references module.modules(module_guid)
    on delete set null,

    constraint fk_module_fault_finding_questions_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_module_fault_finding_questions_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_module_fault_finding_questions_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_module_fault_finding_questions_created_by
    on module.module_fault_finding_questions(created_by_user_guid);

create index if not exists idx_module_fault_finding_questions_updated_by
    on module.module_fault_finding_questions(updated_by_user_guid);

create index if not exists idx_module_fault_finding_questions_deleted_by
    on module.module_fault_finding_questions(deleted_by_user_guid);

create index if not exists idx_module_fault_finding_questions_module_guid
    on module.module_fault_finding_questions(fk_module_guid);

create table if not exists module.module_fault_finding_question_students
(
    module_fault_finding_question_student_guid   uuid        primary key default gen_random_uuid(),
    completion_date                              timestamptz not null,
    is_correct                                   boolean     not null,
    fk_fault_finding_question_guid               uuid        not null,
    fk_student_guid                              uuid        not null,

    constraint fk_module_fault_finding_question_students_question
    foreign key (fk_fault_finding_question_guid)
    references module.module_fault_finding_questions(fault_finding_question_guid)
    on delete cascade,

    constraint fk_module_fault_finding_question_students_user
    foreign key (fk_student_guid)
    references public.users(user_guid)
    on delete cascade
    );

create index if not exists idx_module_fault_finding_question_students_question_guid
    on module.module_fault_finding_question_students(fk_fault_finding_question_guid);

create index if not exists idx_module_fault_finding_question_students_student_guid
    on module.module_fault_finding_question_students(fk_student_guid);

create table if not exists module.module_fault_finding_question_student_answers
(
    module_fault_finding_question_student_answer_guid    uuid        primary key default gen_random_uuid(),
    answer                                               text        not null,
    is_correct                                           boolean     not null,
    submission_date                                      timestamptz not null,
    fk_fault_finding_question_guid                       uuid        not null,
    fk_student_guid                                      uuid        not null,

    constraint fk_module_fault_finding_question_student_answers_question
    foreign key (fk_fault_finding_question_guid)
    references module.module_fault_finding_questions(fault_finding_question_guid)
    on delete cascade,

    constraint fk_module_fault_finding_question_student_answers_user
    foreign key (fk_student_guid)
    references public.users(user_guid)
    on delete cascade
    );

create index if not exists idx_module_fault_finding_question_student_answers_question_guid
    on module.module_fault_finding_question_student_answers(fk_fault_finding_question_guid);

create index if not exists idx_module_fault_finding_question_student_answers_student_guid
    on module.module_fault_finding_question_student_answers(fk_student_guid);

create table if not exists module.module_multiple_choice_questions
(
    module_multiple_choice_question_guid    uuid        primary key default gen_random_uuid(),
    module_multiple_choice_question_id      int         generated always as identity unique,
    question                                text        not null,
    is_unique                               boolean     not null,
    fk_module_guid                          uuid,
    created_at                              timestamptz not null default now(),
    created_by_user_guid                    uuid,
    updated_at                              timestamptz,
    updated_by_user_guid                    uuid,
    deleted_at                              timestamptz,
    deleted_by_user_guid                    uuid,

    constraint fk_module_multiple_choice_questions_module
    foreign key (fk_module_guid)
    references module.modules(module_guid)
    on delete set null,

    constraint fk_module_multiple_choice_questions_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_module_multiple_choice_questions_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_module_multiple_choice_questions_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_module_multiple_choice_questions_created_by
    on module.module_multiple_choice_questions(created_by_user_guid);

create index if not exists idx_module_multiple_choice_questions_updated_by
    on module.module_multiple_choice_questions(updated_by_user_guid);

create index if not exists idx_module_multiple_choice_questions_deleted_by
    on module.module_multiple_choice_questions(deleted_by_user_guid);

create index if not exists idx_module_multiple_choice_questions_module_guid
    on module.module_multiple_choice_questions(fk_module_guid);

create table if not exists module.module_multiple_choice_question_solutions
(
    module_multiple_choice_question_solution_guid    uuid    primary key default gen_random_uuid(),
    answer                                           text    not null,
    is_right                                         boolean not null,
    fk_multiple_choice_question_guid                 uuid    not null,

    constraint fk_module_multiple_choice_question_solutions_question
    foreign key (fk_multiple_choice_question_guid)
    references module.module_multiple_choice_questions(module_multiple_choice_question_guid)
    on delete cascade
    );

create index if not exists idx_module_multiple_choice_question_solutions_question_guid
    on module.module_multiple_choice_question_solutions(fk_multiple_choice_question_guid);

create table if not exists module.module_multiple_choice_question_students
(
    module_multiple_choice_question_student_guid uuid        primary key default gen_random_uuid(),
    completion_date                              timestamptz not null,
    is_correct                                   boolean     not null,
    fk_student_guid                              uuid        not null,
    fk_multiple_choice_question_guid             uuid        not null,

    constraint fk_module_multiple_choice_question_students_user
    foreign key (fk_student_guid)
    references public.users(user_guid)
    on delete cascade,

    constraint fk_module_multiple_choice_question_students_question
    foreign key (fk_multiple_choice_question_guid)
    references module.module_multiple_choice_questions(module_multiple_choice_question_guid)
    on delete cascade
    );

create index if not exists idx_module_multiple_choice_question_students_student_guid
    on module.module_multiple_choice_question_students(fk_student_guid);

create index if not exists idx_module_multiple_choice_question_students_question_guid
    on module.module_multiple_choice_question_students(fk_multiple_choice_question_guid);

create table if not exists module.module_multiple_choice_question_answer_students
(
    module_multiple_choice_question_answer_student_guid  uuid    primary key default gen_random_uuid(),
    fk_multiple_choice_question_solution_guid            uuid    not null,
    fk_student_guid                                      uuid    not null,

    constraint fk_module_multiple_choice_question_answer_students_solution
    foreign key (fk_multiple_choice_question_solution_guid)
    references module.module_multiple_choice_question_solutions(module_multiple_choice_question_solution_guid)
    on delete cascade,

    constraint fk_module_multiple_choice_question_answer_students_user
    foreign key (fk_student_guid)
    references public.users(user_guid)
    on delete cascade
    );

create index if not exists idx_module_mc_answer_students_solution_guid
    on module.module_multiple_choice_question_answer_students(fk_multiple_choice_question_solution_guid);

create index if not exists idx_module_mc_answer_students_student_guid
    on module.module_multiple_choice_question_answer_students(fk_student_guid);

create table if not exists module.module_freetext_questions
(
    module_freetext_question_guid   uuid        primary key default gen_random_uuid(),
    module_freetext_question_id     int         generated always as identity unique,
    question                        text        not null,
    key_solution                    text        not null,
    is_unique                       boolean     not null,
    fk_module_guid                  uuid,
    created_at                      timestamptz not null default now(),
    created_by_user_guid            uuid,
    updated_at                      timestamptz,
    updated_by_user_guid            uuid,
    deleted_at                      timestamptz,
    deleted_by_user_guid            uuid,

    constraint fk_module_freetext_questions_module
    foreign key (fk_module_guid)
    references module.modules(module_guid)
    on delete set null,

    constraint fk_module_freetext_questions_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_module_freetext_questions_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_module_freetext_questions_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_module_freetext_questions_created_by
    on module.module_freetext_questions(created_by_user_guid);

create index if not exists idx_module_freetext_questions_updated_by
    on module.module_freetext_questions(updated_by_user_guid);

create index if not exists idx_module_freetext_questions_deleted_by
    on module.module_freetext_questions(deleted_by_user_guid);

create index if not exists idx_module_freetext_questions_module_guid
    on module.module_freetext_questions(fk_module_guid);

create table if not exists module.freetext_question_students
(
    freetext_question_student_guid      uuid        primary key default gen_random_uuid(),
    completion_date                     timestamptz not null,
    is_correct                          boolean     not null,
    student_answer                      text        not null,
    fk_module_freetext_question_guid    uuid        not null,
    fk_student_guid                     uuid        not null,

    constraint fk_freetext_question_students_question
    foreign key (fk_module_freetext_question_guid)
    references module.module_freetext_questions(module_freetext_question_guid)
    on delete cascade,

    constraint fk_freetext_question_students_user
    foreign key (fk_student_guid)
    references public.users(user_guid)
    on delete cascade
    );

create index if not exists idx_freetext_question_students_question_guid
    on module.freetext_question_students(fk_module_freetext_question_guid);

create index if not exists idx_freetext_question_students_student_guid
    on module.freetext_question_students(fk_student_guid);

create table if not exists module.module_sort_tasks
(
    module_sort_task_guid   uuid        primary key default gen_random_uuid(),
    module_sort_task_id     int         generated always as identity unique,
    title                   text        not null,
    description             text        not null,
    is_unique               boolean     not null,
    fk_module_guid          uuid,
    created_at              timestamptz not null default now(),
    created_by_user_guid    uuid,
    updated_at              timestamptz,
    updated_by_user_guid    uuid,
    deleted_at              timestamptz,
    deleted_by_user_guid    uuid,

    constraint fk_module_sort_tasks_module
    foreign key (fk_module_guid)
    references module.modules(module_guid)
    on delete set null,

    constraint fk_module_sort_tasks_created_by
    foreign key (created_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_module_sort_tasks_updated_by
    foreign key (updated_by_user_guid)
    references public.users(user_guid)
    on delete restrict,

    constraint fk_module_sort_tasks_deleted_by
    foreign key (deleted_by_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_module_sort_tasks_created_by
    on module.module_sort_tasks(created_by_user_guid);

create index if not exists idx_module_sort_tasks_updated_by
    on module.module_sort_tasks(updated_by_user_guid);

create index if not exists idx_module_sort_tasks_deleted_by
    on module.module_sort_tasks(deleted_by_user_guid);

create index if not exists idx_module_sort_tasks_module_guid
    on module.module_sort_tasks(fk_module_guid);

create table if not exists module.sort_task_solutions
(
    sort_task_solution_guid     uuid        primary key default gen_random_uuid(),
    answer                      text        not null,
    sort_order                  smallint    not null,
    fk_module_sort_task_guid    uuid        not null,

    constraint fk_sort_task_solutions_sort_task
    foreign key (fk_module_sort_task_guid)
    references module.module_sort_tasks(module_sort_task_guid)
    on delete cascade
    );

create index if not exists idx_sort_task_solutions_sort_task_guid
    on module.sort_task_solutions(fk_module_sort_task_guid);

create table if not exists module.module_sort_task_student_answers
(
    module_sort_task_student_answer_guid    uuid        primary key default gen_random_uuid(),
    answer                                  text        not null,
    sort_order                              smallint    not null,
    fk_student_guid                         uuid        not null,
    fk_module_sort_task_guid                uuid        not null,

    constraint fk_module_sort_task_student_answers_user
    foreign key (fk_student_guid)
    references public.users(user_guid)
    on delete cascade,

    constraint fk_module_sort_task_student_answers_sort_task
    foreign key (fk_module_sort_task_guid)
    references module.module_sort_tasks(module_sort_task_guid)
    on delete cascade
    );

create index if not exists idx_module_sort_task_student_answers_student_guid
    on module.module_sort_task_student_answers(fk_student_guid);

create index if not exists idx_module_sort_task_student_answers_sort_task_guid
    on module.module_sort_task_student_answers(fk_module_sort_task_guid);

create table if not exists module.module_sort_task_students
(
    module_sort_task_student_guid   uuid        primary key default gen_random_uuid(),
    completion_date                 timestamptz not null,
    is_correct                      boolean     not null,
    fk_student_guid                 uuid        not null,
    fk_module_sort_task_guid        uuid        not null,

    constraint fk_module_sort_task_students_user
    foreign key (fk_student_guid)
    references public.users(user_guid)
    on delete cascade,

    constraint fk_module_sort_task_students_sort_task
    foreign key (fk_module_sort_task_guid)
    references module.module_sort_tasks(module_sort_task_guid)
    on delete cascade
    );

create index if not exists idx_module_sort_task_students_student_guid
    on module.module_sort_task_students(fk_student_guid);

create index if not exists idx_module_sort_task_students_sort_task_guid
    on module.module_sort_task_students(fk_module_sort_task_guid);

create table if not exists system.outobx_events
(
    id uuid primary key default gen_random_uuid(),
    type smallint not null,
    payload text not null,
    created_at timestamptz not null,
    processed_at timestamptz not null,
    error text not null
    );
