/**
 * TopList component displays a ranked list with progress bars.
 * Used for showing top pages, browsers, cities, etc.
 */

interface TopListProps {
  /** Section title */
  title: string;
  /** Array of items with name and count */
  items: { name: string; count: number }[];
}

export function TopList({ title, items }: TopListProps) {
  // Find max count for calculating progress bar widths
  const maxCount = Math.max(...items.map((i) => i.count), 1);

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h3 className="text-sm font-medium text-gray-500 mb-4">{title}</h3>
      <ul className="space-y-3">
        {items.map((item, index) => (
          <li key={index}>
            <div className="flex justify-between text-sm mb-1">
              <span className="text-gray-700 truncate">{item.name}</span>
              <span className="text-gray-500">{item.count}</span>
            </div>
            <div className="w-full bg-gray-100 rounded-full h-2">
              <div
                className="bg-blue-500 h-2 rounded-full"
                style={{ width: `${(item.count / maxCount) * 100}%` }}
              />
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
