import { IAveragePrice } from '../models/campaigns';

/**
 * Formats average prices per currency into a compact KPI-friendly label.
 * Single currency:   "273 EUR"
 * Multiple:          "273 EUR / 2442 UAH"
 */
export function formatAveragePrices(prices: IAveragePrice[]): string {
  if (prices.length === 0) return '—';
  return prices
    .map(p => `${Math.round(p.amount)} ${p.currency}`)
    .join(' / ');
}
