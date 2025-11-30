/**
 * StatCard component displays a single metric with title and value.
 * Used for showing totals like events, users, sessions.
 */

interface StatCardProps {
  /** Label describing the metric */
  title: string;
  /** Numeric or string value to display */
  value: number | string;
}

export function StatCard({ title, value }: StatCardProps) {
  return (
    <div className="bg-white rounded-lg shadow p-6">
      <p className="text-sm text-gray-500 mb-1">{title}</p>
      <p className="text-3xl font-bold text-gray-900">
        {typeof value === "number" ? value.toLocaleString() : value}
      </p>
    </div>
  );
}
