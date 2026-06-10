import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import type { PriceItem } from '../types/PriceItem';

export function usePriceSocket() {
  const [items, setItems] = useState<PriceItem[]>([]);
  const [isSubscribed, setIsSubscribed] = useState(false);
  const [isConnected, setIsConnected] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5000/hubs/prices')
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    connection.on('ReceiveInitialPrices', (data: PriceItem[]) => {
      const sorted = [...data]
        .sort((a, b) => a.id - b.id)
        .map(item => ({ ...item, direction: 'same' as const }));
      setItems(sorted);
    });

    connection.on('ReceivePriceUpdate', (updates: PriceItem[]) => {
      // NOTE: Array scan is O(n) — correct for a fixed set of 10 items.
      // For production scale (10,000+ instruments), refactor state to
      // Map<number, PriceItem> for O(1) merge lookups.
      setItems(prev => {
        const next = prev.map(existing => {
          const update = updates.find(u => u.id === existing.id);
          if (!update) return existing;
          const direction: PriceItem['direction'] =
            update.price > existing.price ? 'up' :
            update.price < existing.price ? 'down' : 'same';
          return { ...update, direction };
        });
        return next;
      });
    });

    connection.onclose(() => setIsConnected(false));

    connection.start()
      .then(() => setIsConnected(true))
      .catch((error) => {
        console.error(error);
        setIsConnected(false);
      });
    
    return () => {
      connection.stop();
      setIsConnected(false);
    };
  }, []);

  const subscribe = async () => {
    await connectionRef.current?.invoke('SubscribeToPrices');
    setIsSubscribed(true);
  };

  const unsubscribe = async () => {
    await connectionRef.current?.invoke('UnsubscribeFromPrices');
    setIsSubscribed(false);
  };

  return { items, isSubscribed, isConnected, subscribe, unsubscribe };
}
