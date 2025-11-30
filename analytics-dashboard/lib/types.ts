export interface AnalyticsEvent {
  partitionKey: string;
  rowKey: string;
  eventType: string;
  timestampUtc: string;
  userId: string;
  sessionId: string;
  url?: string;
  referrer?: string;
  browser?: string;
  device?: string;
  screenSize?: string;
  ipAddress?: string;
  city?: string;
  country?: string;
  serverTimestamp: string;
  processedTimestamp?: string;
  batchId?: string;
}

export interface DashboardStats {
  totalEvents: number;
  uniqueUsers: number;
  uniqueSessions: number;
  topPages: { url: string; count: number }[];
  topBrowsers: { browser: string; count: number }[];
  topCities: { city: string; count: number }[];
  eventsByHour: { hour: string; count: number }[];
}
