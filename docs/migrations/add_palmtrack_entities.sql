-- ============================================================
-- Migration: AddPalmTrackEntities (2026-08-29)
-- Generated from: src/Zorvian.Infrastructure/Migrations/
-- Apply in Neon SQL Editor
-- ============================================================

-- 1. ExternalIdentityMappings (PalmTrack org → Zorvian tenant)
CREATE TABLE IF NOT EXISTS "ExternalIdentityMappings" (
    "Id" uuid NOT NULL,
    "PalmTrackOrgId" text NOT NULL,
    "ZorvianTenantId" text NOT NULL,
    "PalmTrackOrgName" text NULL,
    "ZorvianTenantName" text NULL,
    "IsActive" boolean NOT NULL,
    "LastSyncedAt" timestamp with time zone NOT NULL,
    "TenantId" text NOT NULL,
    "CompanyId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    "UpdatedBy" text NULL,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_ExternalIdentityMappings" PRIMARY KEY ("Id")
);

-- 2. FleetDriverAliases (driver name matching)
CREATE TABLE IF NOT EXISTS "FleetDriverAliases" (
    "Id" uuid NOT NULL,
    "DriverId" uuid NOT NULL,
    "ExternalSystem" text NOT NULL,
    "ExternalName" text NOT NULL,
    "ExternalDriverId" text NULL,
    "MatchType" text NOT NULL,
    "IsPrimary" boolean NOT NULL,
    "MatchCount" integer NOT NULL,
    "TenantId" text NOT NULL,
    "CompanyId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    "UpdatedBy" text NULL,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_FleetDriverAliases" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FleetDriverAliases_Drivers_DriverId" FOREIGN KEY ("DriverId") REFERENCES "Drivers" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_FleetDriverAliases_DriverId" ON "FleetDriverAliases" ("DriverId");

-- 3. FleetExternalReferences (entity mapping between systems)
CREATE TABLE IF NOT EXISTS "FleetExternalReferences" (
    "Id" uuid NOT NULL,
    "ExternalSystem" text NOT NULL,
    "EntityType" text NOT NULL,
    "EntityId" uuid NOT NULL,
    "ExternalId" text NOT NULL,
    "ExternalPayload" text NULL,
    "LastSyncAt" timestamp with time zone NULL,
    "SyncDirection" text NOT NULL,
    "Status" text NOT NULL,
    "LastError" text NULL,
    "ConsecutiveFailures" integer NOT NULL,
    "TenantId" text NOT NULL,
    "CompanyId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    "UpdatedBy" text NULL,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_FleetExternalReferences" PRIMARY KEY ("Id")
);

-- 4. PalmTrackWebhookDlqs (dead letter queue)
CREATE TABLE IF NOT EXISTS "PalmTrackWebhookDlqs" (
    "Id" uuid NOT NULL,
    "IdempotencyKey" text NOT NULL,
    "Event" text NOT NULL,
    "OrganizationId" text NOT NULL,
    "Payload" text NULL,
    "Error" text NOT NULL,
    "FailedAt" timestamp with time zone NOT NULL,
    "RetryCount" integer NOT NULL,
    "IsResolved" boolean NOT NULL,
    "ResolvedAt" timestamp with time zone NULL,
    "ResolvedBy" text NULL,
    "TenantId" text NOT NULL,
    "CompanyId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    "UpdatedBy" text NULL,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_PalmTrackWebhookDlqs" PRIMARY KEY ("Id")
);

-- 5. PalmTrackWebhookLogs (delivery audit trail)
CREATE TABLE IF NOT EXISTS "PalmTrackWebhookLogs" (
    "Id" uuid NOT NULL,
    "IdempotencyKey" text NOT NULL,
    "Event" text NOT NULL,
    "OrganizationId" text NOT NULL,
    "ZorvianTenantId" uuid NULL,
    "Payload" text NULL,
    "Status" text NOT NULL,
    "HttpStatusCode" integer NULL,
    "Error" text NULL,
    "ReceivedAt" timestamp with time zone NOT NULL,
    "ProcessedAt" timestamp with time zone NULL,
    "DurationMs" integer NULL,
    "TenantId" text NOT NULL,
    "CompanyId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    "UpdatedBy" text NULL,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_PalmTrackWebhookLogs" PRIMARY KEY ("Id")
);

-- 6. PalmTrackWebhookSecrets (HMAC secrets per org)
CREATE TABLE IF NOT EXISTS "PalmTrackWebhookSecrets" (
    "Id" uuid NOT NULL,
    "OrganizationId" text NOT NULL,
    "SecretHash" text NOT NULL,
    "SecretPrefix" text NOT NULL,
    "ValidFrom" timestamp with time zone NOT NULL,
    "ValidTo" timestamp with time zone NULL,
    "IsActive" boolean NOT NULL,
    "TenantId" text NOT NULL,
    "CompanyId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    "UpdatedBy" text NULL,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_PalmTrackWebhookSecrets" PRIMARY KEY ("Id")
);

-- ============================================================
-- Register migration in EF Core history table
-- ============================================================
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260829232511_AddPalmTrackEntities', '9.0.0')
ON CONFLICT DO NOTHING;
