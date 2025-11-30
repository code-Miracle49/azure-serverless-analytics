/**
 * API route to fetch analytics statistics from Azure Table Storage.
 * GET /api/analytics?days=7
 */

import { NextResponse } from "next/server";
import { getStats } from "@/lib/table-storage";

export async function GET(request: Request) {
  try {
    // Parse days parameter from query string (default: 7)
    const { searchParams } = new URL(request.url);
    const days = parseInt(searchParams.get("days") ?? "7", 10);

    const stats = await getStats(days);
    return NextResponse.json(stats);
  } catch (error) {
    console.error("Failed to fetch analytics:", error);
    return NextResponse.json(
      { error: "Failed to fetch analytics" },
      { status: 500 }
    );
  }
}
