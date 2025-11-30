import { TableClient, TableEntityResult } from "@azure/data-tables";
import { AnalyticsEvent, DashboardStats } from "./types";

/**
 * Creates a TableClient instance for the AnalyticsEvents table.
 * Reads connection string from environment variables.
 */
function getTableClient(): TableClient {
  const connectionString = process.env.AZURE_STORAGE_CONNECTION_STRING;
  if (!connectionString) {
    throw new Error("AZURE_STORAGE_CONNECTION_STRING not configured");
  }
  return TableClient.fromConnectionString(connectionString, "AnalyticsEvents");
}

/**
 * Fetches recent events from Table Storage.
 * @param limit - Maximum number of events to return (default: 100)
 * @returns Array of analytics events
 */
export async function getRecentEvents(
  limit: number = 100
): Promise<AnalyticsEvent[]> {
  const client = getTableClient();
  const events: AnalyticsEvent[] = [];

  // Filter by today's partition key (YYYYMMDD format)
  const today = new Date().toISOString().slice(0, 10).replace(/-/g, "");

  const iterator = client.listEntities<Record<string, unknown>>({
    queryOptions: {
      filter: `PartitionKey ge '${today}'`,
    },
  });

  for await (const entity of iterator) {
    events.push(mapEntity(entity));
    if (events.length >= limit) break;
  }

  return events;
}

/**
 * Fetches and aggregates statistics for the dashboard.
 * @param days - Number of days to include (default: 7)
 * @returns Aggregated dashboard statistics
 */
export async function getStats(days: number = 7): Promise<DashboardStats> {
  const client = getTableClient();
  const events: AnalyticsEvent[] = [];

  // Calculate start partition key based on days parameter
  const startDate = new Date();
  startDate.setDate(startDate.getDate() - days);
  const startPartition = startDate.toISOString().slice(0, 10).replace(/-/g, "");

  const iterator = client.listEntities<Record<string, unknown>>({
    queryOptions: {
      filter: `PartitionKey ge '${startPartition}'`,
    },
  });

  for await (const entity of iterator) {
    events.push(mapEntity(entity));
  }

  return calculateStats(events);
}

/**
 * Maps a Table Storage entity to our AnalyticsEvent interface.
 * Handles PascalCase property names from ASCDataAccessLibrary.
 */
function mapEntity(
  entity: TableEntityResult<Record<string, unknown>>
): AnalyticsEvent {
  return {
    partitionKey: (entity.partitionKey as string) ?? "",
    rowKey: (entity.rowKey as string) ?? "",
    eventType: (entity.EventType as string) ?? "",
    timestampUtc: (entity.TimestampUtc as string) ?? "",
    userId: (entity.UserId as string) ?? "",
    sessionId: (entity.SessionId as string) ?? "",
    url: entity.Url as string | undefined,
    referrer: entity.Referrer as string | undefined,
    browser: entity.Browser as string | undefined,
    device: entity.Device as string | undefined,
    screenSize: entity.ScreenSize as string | undefined,
    ipAddress: entity.IpAddress as string | undefined,
    city: entity.City as string | undefined,
    country: entity.Country as string | undefined,
    serverTimestamp: (entity.ServerTimestamp as string) ?? "",
    processedTimestamp: entity.ProcessedTimestamp as string | undefined,
    batchId: entity.BatchId as string | undefined,
  };
}

/**
 * Calculates aggregated statistics from raw events.
 * @param events - Array of analytics events to aggregate
 * @returns Dashboard statistics object
 */
function calculateStats(events: AnalyticsEvent[]): DashboardStats {
  const uniqueUsers = new Set(events.map((e) => e.userId));
  const uniqueSessions = new Set(events.map((e) => e.sessionId));

  // Count occurrences for each dimension
  const pageCounts = countBy(events, (e) => e.url ?? "unknown");
  const browserCounts = countBy(events, (e) => e.browser ?? "unknown");
  const cityCounts = countBy(events, (e) => e.city ?? "unknown");
  const hourCounts = countBy(events, (e) => {
    const date = new Date(e.timestampUtc);
    return `${date.getHours().toString().padStart(2, "0")}:00`;
  });

  return {
    totalEvents: events.length,
    uniqueUsers: uniqueUsers.size,
    uniqueSessions: uniqueSessions.size,
    topPages: toSortedArray(pageCounts, 5).map(([url, count]) => ({
      url,
      count,
    })),
    topBrowsers: toSortedArray(browserCounts, 5).map(([browser, count]) => ({
      browser,
      count,
    })),
    topCities: toSortedArray(cityCounts, 5).map(([city, count]) => ({
      city,
      count,
    })),
    eventsByHour: Object.entries(hourCounts)
      .map(([hour, count]) => ({ hour, count }))
      .sort((a, b) => a.hour.localeCompare(b.hour)),
  };
}

/**
 * Groups items by a key function and counts occurrences.
 * @param items - Array of items to group
 * @param keyFn - Function to extract the grouping key
 * @returns Object with keys and their counts
 */
function countBy<T>(
  items: T[],
  keyFn: (item: T) => string
): Record<string, number> {
  return items.reduce((acc, item) => {
    const key = keyFn(item);
    acc[key] = (acc[key] ?? 0) + 1;
    return acc;
  }, {} as Record<string, number>);
}

/**
 * Converts count object to sorted array, limited to top N items.
 * @param counts - Object with keys and counts
 * @param limit - Maximum number of items to return
 * @returns Sorted array of [key, count] tuples
 */
function toSortedArray(
  counts: Record<string, number>,
  limit: number
): [string, number][] {
  return Object.entries(counts)
    .sort((a, b) => b[1] - a[1])
    .slice(0, limit);
}
