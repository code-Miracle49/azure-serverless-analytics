/**
 * Dashboard component fetches analytics data and renders all visualizations.
 * This is the main container component for the analytics dashboard.
 */

"use client";

import { useEffect, useState } from "react";
import { DashboardStats } from "@/lib/types";
import { StatCard } from "./StatCard";
import { TopList } from "./TopList";
import { EventsChart } from "./EventsChart";

export function Dashboard() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchStats() {
      try {
        const response = await fetch("/api/analytics?days=7");
        if (!response.ok) throw new Error("Failed to fetch");
        const data = await response.json();
        setStats(data);
      } catch (err) {
        setError("Failed to load analytics data");
      } finally {
        setLoading(false);
      }
    }

    fetchStats();
  }, []);

  // Loading state
  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <p className="text-gray-500">Loading analytics...</p>
      </div>
    );
  }

  // Error state
  if (error || !stats) {
    return (
      <div className="flex items-center justify-center h-64">
        <p className="text-red-500">{error ?? "No data available"}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Top stats row */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <StatCard title="Total Events" value={stats.totalEvents} />
        <StatCard title="Unique Users" value={stats.uniqueUsers} />
        <StatCard title="Unique Sessions" value={stats.uniqueSessions} />
      </div>

      {/* Events chart */}
      <EventsChart data={stats.eventsByHour} />

      {/* Top lists row */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <TopList
          title="Top Pages"
          items={stats.topPages.map((p) => ({ name: p.url, count: p.count }))}
        />
        <TopList
          title="Top Browsers"
          items={stats.topBrowsers.map((b) => ({
            name: b.browser,
            count: b.count,
          }))}
        />
        <TopList
          title="Top Cities"
          items={stats.topCities.map((c) => ({ name: c.city, count: c.count }))}
        />
      </div>
    </div>
  );
}
