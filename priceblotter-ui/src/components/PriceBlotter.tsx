// 1. Imports
import { AgGridReact } from 'ag-grid-react';
import { AllCommunityModule, ModuleRegistry, themeBalham, colorSchemeDark } from 'ag-grid-community';
import type { ColDef } from 'ag-grid-community';
import { usePriceSocket } from '../hooks/usePriceSocket';
import type { PriceItem } from '../types/PriceItem';

// 2. Module Registration
ModuleRegistry.registerModules([AllCommunityModule]);

// 3. Theme
const darkTheme = themeBalham.withPart(colorSchemeDark);

// 4. Helper Functions — before columnDefs
const formatPrice = ({ value }: { value: number }) =>
  value != null ? value.toFixed(4) : '';

const formatTime = ({ value }: { value: string }) =>
  value ? new Date(value).toTimeString().slice(0, 8) : '';

// 5. Column Definitions — uses formatPrice, formatTime
const columnDefs: ColDef<PriceItem>[] = [
  { headerName: 'ID',         field: 'id', width: 70 },
  { headerName: 'Name',       field: 'name', flex: 2 },
  { headerName: 'Price',      field: 'price', flex: 1 , valueFormatter: formatPrice, enableCellChangeFlash: true },
  { headerName: 'Direction',  field: 'direction', width: 70,
    cellStyle: { textAlign: 'center' },
    cellRenderer: ({ value }: { value: PriceItem['direction'] }) => {
      if (value === 'up')   return <span style={{ color: '#4ade80' }}>▲</span>;
      if (value === 'down') return <span style={{ color: '#f87171' }}>▼</span>;
      return <span style={{ color: '#888' }}>—</span>;
    }
  },
  { headerName: 'Updated At', field: 'updatedAt', width: 100,
    valueFormatter: formatTime },
];

// 6. Main Component — uses columnDefs
export default function PriceBlotter() {
  const { items, isSubscribed, isConnected, subscribe, unsubscribe } = usePriceSocket();

  return (
    <div>
      {/* Toolbar — buttons left, connection status right */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
        <div style={{paddingLeft: 10 }}>
          <button onClick={subscribe}   disabled={isSubscribed}>Subscribe</button>
          <button onClick={unsubscribe} disabled={!isSubscribed}>Unsubscribe</button>
        </div>
        <div style={{ alignItems: 'center', gap: 12, paddingRight: 10 }}>
          {!isConnected && (
            <span style={{ fontSize: 12, color: 'red' }}>Disconnected</span>
          )}
          {isSubscribed && (
            <span style={{ fontSize: 12, color: 'lightgreen' }}>Subscribed</span>
          )}
        </div>
      </div>

      {/* Price blotter grid */}
      <div className="grid-wrapper">
        <AgGridReact
          theme={darkTheme}
          rowData={items}
          columnDefs={columnDefs}
          getRowId={(params) => params.data.id.toString()}
          domLayout='autoHeight'
        />
      </div>
    </div>
  );
}