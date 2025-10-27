# CAN異常検出管理システム - 要求仕様書

## Introduction

CAN異常検出管理システムは、自動車業界におけるCAN信号の異常検出ロジック開発・管理・共有を支援するマルチテナント対応のWebアプリケーションです。複数のOEM（自動車メーカー）が独自のデータ空間を持ちながら、業界共通の知見を共有できる仕組みを提供します。

## Glossary

- **System**: CAN異常検出管理システム
- **OEM**: Original Equipment Manufacturer（自動車メーカー）
- **Tenant**: システム内の独立したデータ空間を持つOEM組織
- **CAN Signal**: Controller Area Network通信で使用される信号
- **Detection Logic**: CAN信号の異常を検出するためのアルゴリズムまたはルール
- **Value Object**: 識別子を持たず、属性値によって等価性が判断されるオブジェクト
- **Entity**: 一意の識別子を持ち、ライフサイクルを通じて追跡されるオブジェクト
- **Aggregate Root**: 関連するエンティティ群の整合性を保証する親エンティティ

## Requirements

### Requirement 1: マルチテナント管理

**User Story:** As an OEM administrator, I want to manage my organization's tenant configuration, so that I can maintain data isolation and control access to shared resources.

#### Acceptance Criteria

1. WHEN an OEM administrator creates a new tenant, THE System SHALL create an isolated data space with unique tenant identifier
2. WHEN a tenant is configured, THE System SHALL allow specification of OEM code, company name, country, and contact information
3. WHEN a tenant accesses data, THE System SHALL enforce data isolation ensuring tenant can only access their own data and explicitly shared resources
4. WHERE a tenant has custom features, THE System SHALL store feature configurations as key-value pairs
5. WHEN a tenant is deactivated, THE System SHALL prevent access to tenant data while preserving data integrity

### Requirement 2: CAN信号管理

**User Story:** As a signal engineer, I want to define and manage CAN signal specifications, so that I can maintain accurate signal definitions for anomaly detection.

#### Acceptance Criteria

1. WHEN a signal engineer creates a CAN signal, THE System SHALL require signal name, CAN ID, start bit, length, and data type
2. WHEN a CAN signal is defined, THE System SHALL store signal identifier (name and CAN ID) as a value object
3. WHEN signal specifications are configured, THE System SHALL store specification details (start bit, length, data type, byte order, value range) as a value object
4. WHEN physical value conversion is needed, THE System SHALL store conversion parameters (factor, offset, unit) as a value object
5. WHEN signal timing is configured, THE System SHALL store timing information (cycle time, timeout, send type) as a value object
6. WHEN a signal version is updated, THE System SHALL track version history with major and minor version numbers
7. WHEN a signal belongs to a system category, THE System SHALL associate the signal with the appropriate system type

### Requirement 3: 異常検出ロジック管理

**User Story:** As a detection engineer, I want to create and manage anomaly detection logic, so that I can implement effective anomaly detection algorithms.

#### Acceptance Criteria

1. WHEN a detection engineer creates detection logic, THE System SHALL require logic identity (name, version, OEM code) as a value object
2. WHEN detection logic is specified, THE System SHALL store specification details (detection type, description, target system, complexity) as a value object
3. WHEN logic implementation is provided, THE System SHALL store implementation details (type, content, language, entry point) as a value object
4. WHEN safety classification is required, THE System SHALL store safety information (ASIL level, safety requirement ID, safety goal ID) as a value object
5. WHEN detection parameters are defined, THE System SHALL create parameter entities with name, data type, value, constraints, and description
6. WHEN CAN signals are mapped to detection logic, THE System SHALL create signal mapping entities with signal ID, role, and configuration
7. WHEN detection logic status changes, THE System SHALL track status transitions (draft, testing, approved, deprecated)
8. WHEN sharing level is set, THE System SHALL control visibility (private, OEM-shared, industry-shared)

### Requirement 4: 検出パラメータ管理

**User Story:** As a detection engineer, I want to configure detection parameters with validation constraints, so that I can ensure parameter values are within acceptable ranges.

#### Acceptance Criteria

1. WHEN a parameter is created, THE System SHALL store parameter as an entity with unique identifier
2. WHEN parameter constraints are defined, THE System SHALL store constraints (min/max value, min/max length, pattern, allowed values) as a value object
3. WHEN a parameter value is updated, THE System SHALL validate the value against defined constraints
4. WHEN a parameter is required, THE System SHALL enforce that a value must be provided
5. WHEN a parameter has a default value, THE System SHALL use the default when no value is specified

### Requirement 5: CAN信号マッピング管理

**User Story:** As a detection engineer, I want to map CAN signals to detection logic with specific roles, so that I can define which signals are used for detection.

#### Acceptance Criteria

1. WHEN a signal mapping is created, THE System SHALL store mapping as an entity with signal ID and role
2. WHEN mapping configuration is defined, THE System SHALL store configuration (scaling factor, offset, filter expression) as a value object
3. WHEN a signal is marked as required, THE System SHALL enforce that the signal must be present for detection execution
4. WHEN multiple signals are mapped, THE System SHALL maintain the collection of mappings within the detection logic aggregate

### Requirement 6: プロジェクト管理

**User Story:** As a project manager, I want to manage anomaly detection projects with milestones and team members, so that I can track project progress and coordinate team activities.

#### Acceptance Criteria

1. WHEN a project is created, THE System SHALL require project code, name, vehicle model, model year, and primary system
2. WHEN project configuration is set, THE System SHALL store configuration (priority, confidentiality, tags, custom settings) as a value object
3. WHEN project milestones are defined, THE System SHALL create milestone entities with name, due date, status, and configuration
4. WHEN team members are assigned, THE System SHALL create member entities with user ID, role, and configuration
5. WHEN project status changes, THE System SHALL track status transitions (planning, active, on-hold, completed, cancelled)

### Requirement 7: マイルストーン管理

**User Story:** As a project manager, I want to define project milestones with dependencies, so that I can plan and track project phases.

#### Acceptance Criteria

1. WHEN a milestone is created, THE System SHALL store milestone as an entity with name and due date
2. WHEN milestone configuration is defined, THE System SHALL store configuration (critical flag, approval requirement, dependencies) as a value object
3. WHEN a milestone is completed, THE System SHALL record completion date and completing user
4. WHEN milestone dependencies exist, THE System SHALL track dependency relationships between milestones
5. WHEN milestone status changes, THE System SHALL track status transitions (pending, in-progress, completed, cancelled)

### Requirement 8: プロジェクトメンバー管理

**User Story:** As a project manager, I want to manage project team members with roles and permissions, so that I can control access and responsibilities.

#### Acceptance Criteria

1. WHEN a member is added to a project, THE System SHALL store member as an entity with user ID and role
2. WHEN member configuration is defined, THE System SHALL store configuration (permissions, settings, notification preferences) as a value object
3. WHEN a member joins a project, THE System SHALL record the join date
4. WHEN a member leaves a project, THE System SHALL record the leave date and mark member as inactive
5. WHEN member permissions are set, THE System SHALL enforce access control based on configured permissions

### Requirement 9: 異常検出結果管理

**User Story:** As a test engineer, I want to record and analyze anomaly detection results, so that I can evaluate detection logic effectiveness.

#### Acceptance Criteria

1. WHEN detection is executed, THE System SHALL create a detection result entity with detection logic ID, signal ID, and timestamp
2. WHEN input data is recorded, THE System SHALL store input data (signal value, timestamp, additional data) as a value object
3. WHEN detection details are captured, THE System SHALL store details (detection type, trigger condition, execution time, parameters) as a value object
4. WHEN anomaly level is determined, THE System SHALL record the severity level (info, warning, error, critical)
5. WHEN confidence score is calculated, THE System SHALL store the confidence score (0.0 to 1.0)
6. WHEN resolution status changes, THE System SHALL track status transitions (unresolved, investigating, resolved, false-positive)

### Requirement 10: システムカテゴリ管理

**User Story:** As a system administrator, I want to define CAN system categories with configurations, so that I can organize signals by automotive systems.

#### Acceptance Criteria

1. WHEN a system category is created, THE System SHALL require system type and name
2. WHEN category configuration is defined, THE System SHALL store configuration (priority, safety relevance, real-time monitoring requirement) as a value object
3. WHEN custom settings are needed, THE System SHALL store custom settings as key-value pairs
4. WHEN a category is activated or deactivated, THE System SHALL control category availability
5. WHEN display order is set, THE System SHALL use the order for UI presentation

### Requirement 11: OEMカスタマイズ管理

**User Story:** As an OEM engineer, I want to customize shared detection logic and signals for my organization, so that I can adapt industry standards to my specific requirements.

#### Acceptance Criteria

1. WHEN an OEM customizes an entity, THE System SHALL record entity ID, entity type, and OEM code
2. WHEN customization parameters are defined, THE System SHALL store both custom parameters and original parameters
3. WHEN customization is created, THE System SHALL require customization reason and set status to draft
4. WHEN customization is submitted for approval, THE System SHALL change status to pending approval
5. WHEN customization is approved, THE System SHALL record approver ID, approval date, and approval notes
6. WHEN customization is rejected, THE System SHALL record rejection reason and change status to rejected
7. WHEN approved customization is updated, THE System SHALL prevent modification and require new customization
8. WHEN customization becomes obsolete, THE System SHALL mark status as obsolete

### Requirement 12: OEM承認ワークフロー管理

**User Story:** As an OEM approver, I want to review and approve customization requests, so that I can ensure quality and compliance before applying changes.

#### Acceptance Criteria

1. WHEN approval is requested, THE System SHALL record entity ID, entity type, OEM code, requester ID, and request date
2. WHEN approval type is specified, THE System SHALL store approval type (customization, logic deployment, signal modification)
3. WHEN approval reason is provided, THE System SHALL store approval reason and related data
4. WHEN approval is pending, THE System SHALL allow setting due date and priority level
5. WHEN approval is granted, THE System SHALL record approver ID, approval date, and approval notes
6. WHEN approval is rejected, THE System SHALL record rejection reason and change status to rejected
7. WHEN approval is cancelled, THE System SHALL record cancellation reason and change status to cancelled
8. WHEN approval is overdue, THE System SHALL identify and flag overdue approvals

### Requirement 13: 類似パターン検索

**User Story:** As a detection engineer, I want to search for similar CAN signals and detection patterns, so that I can reuse existing knowledge and improve detection logic.

#### Acceptance Criteria

1. WHEN similarity search is requested, THE System SHALL accept search criteria with comparison options
2. WHEN comparing signals, THE System SHALL calculate similarity based on CAN ID, signal name, system type, value range, data length, cycle time, and OEM code
3. WHEN similarity is calculated, THE System SHALL provide detailed similarity breakdown with weighted scores
4. WHEN similar signals are found, THE System SHALL identify matched attributes and differences
5. WHEN recommendation level is determined, THE System SHALL classify as highly recommended, high, medium, low, or not recommended
6. WHEN search results are returned, THE System SHALL sort by similarity score and limit to maximum results
7. WHEN test data is compared, THE System SHALL analyze threshold differences, condition differences, and result differences
8. WHEN comparison recommendations are generated, THE System SHALL provide actionable recommendations with priority levels

### Requirement 14: 異常分析サービス

**User Story:** As a test engineer, I want to analyze anomaly detection patterns and accuracy, so that I can optimize detection logic and improve system performance.

#### Acceptance Criteria

1. WHEN pattern analysis is requested, THE System SHALL analyze anomaly type distribution, level distribution, and frequency patterns
2. WHEN correlations are analyzed, THE System SHALL identify relationships between anomalies across signals
3. WHEN detection accuracy is calculated, THE System SHALL compute precision, recall, F1 score, and confusion matrix
4. WHEN threshold recommendations are generated, THE System SHALL analyze current performance and suggest optimizations
5. WHEN performance metrics are calculated, THE System SHALL provide accuracy by anomaly type and time range
6. WHEN analysis results are returned, THE System SHALL include summary and actionable insights

## Non-Functional Requirements

### NFR 1: パフォーマンス

**User Story:** As a system user, I want fast response times, so that I can work efficiently without delays.

#### Acceptance Criteria

1. WHEN a user queries CAN signals, THE System SHALL return results within 500 milliseconds for up to 10,000 records
2. WHEN detection logic is executed, THE System SHALL complete execution within 100 milliseconds for standard complexity
3. WHEN similarity search is performed, THE System SHALL return results within 2 seconds for up to 1,000 candidate signals
4. WHEN dashboard loads, THE System SHALL display initial data within 1 second
5. WHEN concurrent users access the system, THE System SHALL support at least 100 concurrent users without performance degradation

### NFR 2: セキュリティ

**User Story:** As a security administrator, I want robust security controls, so that I can protect sensitive automotive data.

#### Acceptance Criteria

1. WHEN a user authenticates, THE System SHALL use OAuth 2.0 or OpenID Connect protocols
2. WHEN data is transmitted, THE System SHALL use TLS 1.2 or higher encryption
3. WHEN data is stored, THE System SHALL encrypt sensitive fields using AES-256
4. WHEN authorization is checked, THE System SHALL enforce role-based access control (RBAC)
5. WHEN audit logs are created, THE System SHALL record all data modifications with user ID and timestamp
6. WHEN multi-tenant isolation is enforced, THE System SHALL prevent cross-tenant data access

### NFR 3: スケーラビリティ

**User Story:** As a system administrator, I want the system to scale, so that it can handle growing data volumes and user base.

#### Acceptance Criteria

1. WHEN data volume increases, THE System SHALL support at least 1 million CAN signals per tenant
2. WHEN detection results accumulate, THE System SHALL support at least 10 million detection results per tenant
3. WHEN tenants are added, THE System SHALL support at least 50 tenants without architectural changes
4. WHEN database grows, THE System SHALL implement data archiving for records older than 2 years
5. WHEN load increases, THE System SHALL support horizontal scaling of application servers

### NFR 4: 可用性

**User Story:** As a business stakeholder, I want high system availability, so that users can access the system when needed.

#### Acceptance Criteria

1. WHEN system is operational, THE System SHALL maintain 99.5% uptime during business hours
2. WHEN failures occur, THE System SHALL implement automatic failover for critical services
3. WHEN maintenance is required, THE System SHALL support zero-downtime deployments
4. WHEN backups are performed, THE System SHALL create daily backups with 30-day retention
5. WHEN disaster recovery is needed, THE System SHALL support recovery with RPO of 1 hour and RTO of 4 hours

### NFR 5: 保守性

**User Story:** As a developer, I want maintainable code, so that I can efficiently fix bugs and add features.

#### Acceptance Criteria

1. WHEN code is written, THE System SHALL follow Domain-Driven Design (DDD) principles
2. WHEN components are designed, THE System SHALL maintain clear separation of concerns across layers
3. WHEN tests are created, THE System SHALL achieve at least 80% code coverage for domain logic
4. WHEN documentation is provided, THE System SHALL include API documentation using OpenAPI/Swagger
5. WHEN logging is implemented, THE System SHALL use structured logging with correlation IDs

