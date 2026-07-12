-- ChargePay V3 Database DDL
-- PostgreSQL schema for Identity / Customer / Wallet / Charging Session / Billing / Audit / Notification

CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Schemas
CREATE SCHEMA IF NOT EXISTS customer;
CREATE SCHEMA IF NOT EXISTS identity;
CREATE SCHEMA IF NOT EXISTS wallet;
CREATE SCHEMA IF NOT EXISTS station;
CREATE SCHEMA IF NOT EXISTS charging;
CREATE SCHEMA IF NOT EXISTS telemetry;
CREATE SCHEMA IF NOT EXISTS tariff;
CREATE SCHEMA IF NOT EXISTS billing;
CREATE SCHEMA IF NOT EXISTS audit;
CREATE SCHEMA IF NOT EXISTS notification;
CREATE SCHEMA IF NOT EXISTS core_banking;

-- Types
CREATE TYPE identity.user_status AS ENUM ('ACTIVE', 'INACTIVE', 'LOCKED', 'SUSPENDED');
CREATE TYPE wallet.wallet_status AS ENUM ('ACTIVE', 'INACTIVE', 'BLOCKED');
CREATE TYPE station.station_status AS ENUM ('ACTIVE', 'INACTIVE', 'MAINTENANCE');
CREATE TYPE charging.session_status AS ENUM ('CREATED', 'AUTHORIZED', 'IN_PROGRESS', 'COMPLETED', 'CANCELLED', 'FAILED');
CREATE TYPE billing.invoice_status AS ENUM ('PENDING', 'PAID', 'OVERDUE', 'CANCELLED');

-- Customer table
CREATE TABLE IF NOT EXISTS customer.customers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    person_type VARCHAR(2) NOT NULL CHECK (person_type IN ('PF', 'PJ')),
    first_name VARCHAR(80) NOT NULL,
    last_name VARCHAR(120) NOT NULL,
    document VARCHAR(14) NOT NULL,
    birth_date DATE NOT NULL,
    phone_number VARCHAR(11) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_customers_document UNIQUE (document)
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_customers_document ON customer.customers (document);
CREATE INDEX IF NOT EXISTS idx_customers_name ON customer.customers (lower(first_name || ' ' || last_name));

-- Identity users table
CREATE TABLE IF NOT EXISTS identity.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id UUID NOT NULL,
    email VARCHAR(255) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    status identity.user_status NOT NULL DEFAULT 'ACTIVE',
    last_login_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_users_customer FOREIGN KEY (customer_id) REFERENCES customer.customers (id) ON DELETE CASCADE,
    CONSTRAINT uq_users_email UNIQUE (email)
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON identity.users (email);
CREATE INDEX IF NOT EXISTS idx_users_status ON identity.users (status);
CREATE INDEX IF NOT EXISTS idx_users_customer_id ON identity.users (customer_id);

-- Wallets table
CREATE TABLE IF NOT EXISTS wallet.wallets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id UUID NOT NULL,
    balance NUMERIC(18,2) NOT NULL DEFAULT 0,
    status wallet.wallet_status NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_wallets_customer FOREIGN KEY (customer_id) REFERENCES customer.customers (id) ON DELETE CASCADE,
    CONSTRAINT uq_wallets_customer UNIQUE (customer_id)
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_wallets_customer_id ON wallet.wallets (customer_id);

-- Station table
CREATE TABLE IF NOT EXISTS station.stations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    address VARCHAR(500),
    status station.station_status NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Charging sessions table
CREATE TABLE IF NOT EXISTS charging.charging_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id UUID NOT NULL,
    wallet_id UUID NOT NULL,
    station_id UUID NOT NULL,
    connector_id VARCHAR(50) NOT NULL,
    status charging.session_status NOT NULL DEFAULT 'CREATED',
    started_at TIMESTAMPTZ NOT NULL,
    finished_at TIMESTAMPTZ,
    total_consumption NUMERIC(18,3) NOT NULL DEFAULT 0,
    total_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_charging_sessions_customer FOREIGN KEY (customer_id) REFERENCES customer.customers (id) ON DELETE CASCADE,
    CONSTRAINT fk_charging_sessions_wallet FOREIGN KEY (wallet_id) REFERENCES wallet.wallets (id) ON DELETE CASCADE,
    CONSTRAINT fk_charging_sessions_station FOREIGN KEY (station_id) REFERENCES station.stations (id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_charging_sessions_customer_id ON charging.charging_sessions (customer_id);
CREATE INDEX IF NOT EXISTS idx_charging_sessions_wallet_id ON charging.charging_sessions (wallet_id);
CREATE INDEX IF NOT EXISTS idx_charging_sessions_station_id ON charging.charging_sessions (station_id);

-- Telemetry table
CREATE TABLE IF NOT EXISTS telemetry.session_telemetry (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    charging_session_id UUID NOT NULL,
    recorded_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    voltage NUMERIC(10,2),
    current NUMERIC(10,2),
    power NUMERIC(10,2),
    energy NUMERIC(18,3),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_session_telemetry_charging_session FOREIGN KEY (charging_session_id) REFERENCES charging.charging_sessions (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_session_telemetry_charging_session_id ON telemetry.session_telemetry (charging_session_id);

-- Tariff table
CREATE TABLE IF NOT EXISTS tariff.tariffs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    description TEXT,
    rate NUMERIC(18,4) NOT NULL,
    effective_from TIMESTAMPTZ NOT NULL,
    effective_to TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Billing table
CREATE TABLE IF NOT EXISTS billing.invoices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id UUID NOT NULL,
    charging_session_id UUID,
    amount NUMERIC(18,2) NOT NULL,
    due_date DATE NOT NULL,
    status billing.invoice_status NOT NULL DEFAULT 'PENDING',
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_invoices_customer FOREIGN KEY (customer_id) REFERENCES customer.customers (id) ON DELETE CASCADE,
    CONSTRAINT fk_invoices_charging_session FOREIGN KEY (charging_session_id) REFERENCES charging.charging_sessions (id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_invoices_customer_id ON billing.invoices (customer_id);
CREATE INDEX IF NOT EXISTS idx_invoices_status ON billing.invoices (status);

-- Audit table
CREATE TABLE IF NOT EXISTS audit.audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id UUID,
    user_id UUID,
    event_type VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_audit_logs_customer FOREIGN KEY (customer_id) REFERENCES customer.customers (id) ON DELETE SET NULL,
    CONSTRAINT fk_audit_logs_user FOREIGN KEY (user_id) REFERENCES identity.users (id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_audit_logs_customer_id ON audit.audit_logs (customer_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id ON audit.audit_logs (user_id);

-- Notification table
CREATE TABLE IF NOT EXISTS notification.notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id UUID NOT NULL,
    title VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    sent_at TIMESTAMPTZ,
    CONSTRAINT fk_notifications_customer FOREIGN KEY (customer_id) REFERENCES customer.customers (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_notifications_customer_id ON notification.notifications (customer_id);
CREATE INDEX IF NOT EXISTS idx_notifications_is_read ON notification.notifications (is_read);

-- Event payload example (not a table, but guidance)
-- {
--   "eventType": "usuario-autenticado",
--   "userId": "...",
--   "customerId": "..."
-- }
