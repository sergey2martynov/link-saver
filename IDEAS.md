# MyMemory — Feature Ideas

## Planned Microservices

### AI ClassifierService
Auto-categorizes links (music, video, programming, history, food, etc.) using Claude/OpenAI.
- Kafka: consumes `links.created`, publishes `links.classified`
- LinkService consumes `links.classified` and stores category on the link record
- Complex SQL: filter/aggregate links by category, category distribution per user

### TimelineService
Groups a user's links into auto-detected reading sessions or named eras ("Summer 2025", "Job search").
- Complex SQL: window functions + LAG/LEAD to detect gaps between saves, cluster into sessions
- Supports manual topic labels or auto-grouping by tag similarity
- Kafka: consumes `links.created` to build the timeline incrementally

### SubscriptionService
Users buy a plan (Free / Pro / Team). Quotas and advanced features gated behind subscription.
- Stripe webhook consumer → Kafka `subscription.activated / expired / cancelled`
- Complex SQL: join users ↔ subscriptions ↔ plans, entitlement check in a single query
- Other services check subscription status via internal HTTP or Kafka events

### SharingService
Users create public collections and share them via URL or user invite.
- Complex SQL: ACL/permission checks, JOIN across users ↔ collections ↔ links ↔ members
- Kafka: `collection.shared` → NotificationService
- Teaches: multi-tenancy patterns, access control queries

### ReminderService
"Read this later in 3 days." Scheduled per-link reminders.
- Quartz.NET or polling loop with `WHERE remind_at <= NOW()`
- Kafka: publishes `reminder.triggered` → NotificationService
- Complex SQL: bulk expire/snooze, recurring reminders

### NotificationService
Sends email + Telegram messages. Pure Kafka consumer, no inbound HTTP.
- Consumes events from every other service
- Template engine per event type
- Teaches: fan-out consumer, idempotent delivery (don't send twice on retry)

### ImportService
Import browser bookmarks (Chrome/Firefox HTML export), parse and bulk-create links.
- Kafka: publishes batches of `links.created` after parsing
- Complex SQL: bulk upsert (INSERT ... ON CONFLICT DO NOTHING), deduplication by URL
- Teaches: file upload handling, batch event publishing

### ArchiveService
Saves a plain-text or screenshot snapshot of the page when a link is created (personal Wayback Machine).
- Kafka: consumes `links.created`, fetches page, stores in S3/MinIO
- Publishes `links.archived` with snapshot URL back to LinkService
- Teaches: blob storage integration, long-running async jobs

### AnalyticsService
Tracks link events and generates statistics (top links, user activity, trending tags).
- Kafka: consumes `links.created`, `links.deleted`, `users.registered`
- Complex SQL: CTEs, window functions, DATE_TRUNC aggregations, unnest for tag frequency
- Endpoints: top links, user stats, trending tags
- Best added once other services are producing enough events

---

## Suggested Build Order

| # | Service | Reason |
|---|---------|--------|
| 1 | AI ClassifierService | Simple Kafka in/out loop, immediate visible result |
| 2 | TimelineService | Standalone, meaty SQL (window functions), consumes existing events |
| 3 | NotificationService | Ties everything together, teaches multi-event fan-out |
| 4 | SubscriptionService | More complex (Stripe + entitlement checks), better after Kafka is familiar |
| 5 | SharingService / ReminderService / ImportService | Pick based on interest |
| 6 | AnalyticsService | Add last, once there is rich data from other services |

---

## Kafka Topics Overview

| Topic | Producer | Consumer(s) |
|-------|----------|-------------|
| `links.created` | LinkService | ClassifierService, TimelineService, ArchiveService, ImportService, AnalyticsService |
| `links.deleted` | LinkService | AnalyticsService |
| `links.classified` | ClassifierService | LinkService |
| `links.archived` | ArchiveService | LinkService |
| `users.registered` | UserService | AnalyticsService, NotificationService |
| `subscription.activated` | SubscriptionService | UserService, NotificationService |
| `subscription.expired` | SubscriptionService | UserService, NotificationService |
| `collection.shared` | SharingService | NotificationService |
| `reminder.triggered` | ReminderService | NotificationService |
