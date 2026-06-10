export interface PriceItem {
  id: number;
  name: string;
  price: number;
  updatedAt: string;
  direction?: 'up' | 'down' | 'same';
}
