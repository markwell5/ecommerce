import { useState } from 'react';
import { useQuery, useMutation } from '@apollo/client/react';
import {
  Box,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  TablePagination,
  CircularProgress,
  Alert,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  List,
  ListItem,
  ListItemText,
  Divider,
} from '@mui/material';
import {
  Visibility as ViewIcon,
  LocalShipping as ShipIcon,
  CheckCircle as DeliverIcon,
  Cancel as CancelIcon,
  Undo as ReturnIcon,
} from '@mui/icons-material';
import { GET_ORDERS, CANCEL_ORDER, SHIP_ORDER, DELIVER_ORDER, RETURN_ORDER } from '../graphql/orders';

interface OrderItem {
  productId: number;
  quantity: number;
  unitPrice: number;
}

interface OrderCustomer {
  firstName: string;
  lastName: string;
  email: string;
}

interface OrderRecord {
  orderId: string;
  customerId: string;
  status: string;
  totalAmount: number;
  items: OrderItem[];
  customer: OrderCustomer | null;
  createdAt: string;
  updatedAt: string;
}

const statusColors: Record<string, 'default' | 'info' | 'warning' | 'success' | 'error'> = {
  Placed: 'info',
  Processing: 'warning',
  Shipped: 'warning',
  Delivered: 'success',
  Completed: 'success',
  Cancelled: 'error',
  Returned: 'error',
};

const statusFilters = ['', 'Placed', 'Processing', 'Shipped', 'Delivered', 'Completed', 'Cancelled', 'Returned'];

// Which actions are available for each status
function getActions(status: string): string[] {
  switch (status) {
    case 'Placed':
    case 'Processing':
      return ['ship', 'cancel'];
    case 'Shipped':
      return ['deliver'];
    case 'Delivered':
    case 'Completed':
      return ['return'];
    default:
      return [];
  }
}

export default function OrdersPage() {
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [statusFilter, setStatusFilter] = useState('');

  const [detailOrder, setDetailOrder] = useState<OrderRecord | null>(null);

  const [confirmAction, setConfirmAction] = useState<{ orderId: string; action: string } | null>(null);

  const { data, loading, error, refetch } = useQuery(GET_ORDERS, {
    variables: { page: page + 1, pageSize, status: statusFilter || null },
    fetchPolicy: 'cache-and-network',
  });

  const [cancelOrder, { loading: cancelling }] = useMutation(CANCEL_ORDER);
  const [shipOrder, { loading: shipping }] = useMutation(SHIP_ORDER);
  const [deliverOrder, { loading: delivering }] = useMutation(DELIVER_ORDER);
  const [returnOrder, { loading: returning }] = useMutation(RETURN_ORDER);

  const orders: OrderRecord[] = data?.orders?.items ?? [];
  const totalCount: number = data?.orders?.totalCount ?? 0;
  const mutating = cancelling || shipping || delivering || returning;

  const executeAction = async () => {
    if (!confirmAction) return;
    const { orderId, action } = confirmAction;
    try {
      switch (action) {
        case 'ship': await shipOrder({ variables: { orderId } }); break;
        case 'deliver': await deliverOrder({ variables: { orderId } }); break;
        case 'cancel': await cancelOrder({ variables: { orderId } }); break;
        case 'return': await returnOrder({ variables: { orderId } }); break;
      }
      setConfirmAction(null);
      refetch();
    } catch (err) {
      console.error('Action failed:', err);
    }
  };

  const actionLabel: Record<string, string> = {
    ship: 'Ship',
    deliver: 'Mark Delivered',
    cancel: 'Cancel',
    return: 'Return',
  };

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 3 }}>Orders</Typography>

      <FormControl size="small" sx={{ mb: 2, minWidth: 180 }}>
        <InputLabel>Filter by status</InputLabel>
        <Select
          value={statusFilter}
          label="Filter by status"
          onChange={(e) => { setStatusFilter(e.target.value); setPage(0); }}
        >
          <MenuItem value="">All statuses</MenuItem>
          {statusFilters.filter(Boolean).map((s) => (
            <MenuItem key={s} value={s}>{s}</MenuItem>
          ))}
        </Select>
      </FormControl>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          Failed to load orders: {error.message}
        </Alert>
      )}

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Order ID</TableCell>
              <TableCell>Customer</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Total</TableCell>
              <TableCell>Date</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading && !data ? (
              <TableRow>
                <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                  <CircularProgress />
                </TableCell>
              </TableRow>
            ) : orders.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                  <Typography color="text.secondary">No orders found</Typography>
                </TableCell>
              </TableRow>
            ) : (
              orders.map((order) => (
                <TableRow key={order.orderId} hover>
                  <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                    {order.orderId.slice(0, 8)}...
                  </TableCell>
                  <TableCell>
                    {order.customer ? (
                      <>
                        <Typography variant="body2" fontWeight={500}>
                          {order.customer.firstName} {order.customer.lastName}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {order.customer.email}
                        </Typography>
                      </>
                    ) : (
                      <Typography variant="body2" color="text.secondary">
                        {order.customerId.slice(0, 8)}...
                      </Typography>
                    )}
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={order.status}
                      size="small"
                      color={statusColors[order.status] ?? 'default'}
                    />
                  </TableCell>
                  <TableCell align="right">${order.totalAmount.toFixed(2)}</TableCell>
                  <TableCell>{new Date(order.createdAt).toLocaleDateString()}</TableCell>
                  <TableCell align="right">
                    <IconButton size="small" onClick={() => setDetailOrder(order)} title="View details">
                      <ViewIcon fontSize="small" />
                    </IconButton>
                    {getActions(order.status).includes('ship') && (
                      <IconButton
                        size="small"
                        color="primary"
                        onClick={() => setConfirmAction({ orderId: order.orderId, action: 'ship' })}
                        title="Ship"
                      >
                        <ShipIcon fontSize="small" />
                      </IconButton>
                    )}
                    {getActions(order.status).includes('deliver') && (
                      <IconButton
                        size="small"
                        color="success"
                        onClick={() => setConfirmAction({ orderId: order.orderId, action: 'deliver' })}
                        title="Mark Delivered"
                      >
                        <DeliverIcon fontSize="small" />
                      </IconButton>
                    )}
                    {getActions(order.status).includes('cancel') && (
                      <IconButton
                        size="small"
                        color="error"
                        onClick={() => setConfirmAction({ orderId: order.orderId, action: 'cancel' })}
                        title="Cancel"
                      >
                        <CancelIcon fontSize="small" />
                      </IconButton>
                    )}
                    {getActions(order.status).includes('return') && (
                      <IconButton
                        size="small"
                        color="warning"
                        onClick={() => setConfirmAction({ orderId: order.orderId, action: 'return' })}
                        title="Return"
                      >
                        <ReturnIcon fontSize="small" />
                      </IconButton>
                    )}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
        <TablePagination
          component="div"
          count={totalCount}
          page={page}
          onPageChange={(_, newPage) => setPage(newPage)}
          rowsPerPage={pageSize}
          onRowsPerPageChange={(e) => {
            setPageSize(parseInt(e.target.value, 10));
            setPage(0);
          }}
          rowsPerPageOptions={[5, 10, 25, 50]}
        />
      </TableContainer>

      {/* Order Detail Dialog */}
      <Dialog open={!!detailOrder} onClose={() => setDetailOrder(null)} maxWidth="sm" fullWidth>
        <DialogTitle>
          Order {detailOrder?.orderId.slice(0, 8)}...
          <Chip
            label={detailOrder?.status}
            size="small"
            color={statusColors[detailOrder?.status ?? ''] ?? 'default'}
            sx={{ ml: 1 }}
          />
        </DialogTitle>
        <DialogContent>
          {detailOrder && (
            <>
              <Typography variant="subtitle2" gutterBottom sx={{ mt: 1 }}>
                Customer
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                {detailOrder.customer
                  ? `${detailOrder.customer.firstName} ${detailOrder.customer.lastName} (${detailOrder.customer.email})`
                  : detailOrder.customerId}
              </Typography>

              <Typography variant="subtitle2" gutterBottom>
                Items
              </Typography>
              <List disablePadding dense>
                {detailOrder.items.map((item, i) => (
                  <Box key={i}>
                    <ListItem>
                      <ListItemText
                        primary={`Product #${item.productId}`}
                        secondary={`Qty: ${item.quantity} × $${item.unitPrice.toFixed(2)}`}
                      />
                      <Typography variant="body2">
                        ${(item.quantity * item.unitPrice).toFixed(2)}
                      </Typography>
                    </ListItem>
                    <Divider />
                  </Box>
                ))}
              </List>

              <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2 }}>
                <Typography variant="subtitle2">Total</Typography>
                <Typography variant="subtitle2">${detailOrder.totalAmount.toFixed(2)}</Typography>
              </Box>

              <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
                Created: {new Date(detailOrder.createdAt).toLocaleString()}
                {detailOrder.updatedAt && (
                  <> | Updated: {new Date(detailOrder.updatedAt).toLocaleString()}</>
                )}
              </Typography>
            </>
          )}
        </DialogContent>
      </Dialog>

      {/* Action Confirmation Dialog */}
      <Dialog open={!!confirmAction} onClose={() => setConfirmAction(null)}>
        <DialogTitle>Confirm Action</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to <strong>{confirmAction?.action}</strong> order{' '}
            <code>{confirmAction?.orderId.slice(0, 8)}...</code>?
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmAction(null)}>Cancel</Button>
          <Button
            variant="contained"
            color={confirmAction?.action === 'cancel' || confirmAction?.action === 'return' ? 'error' : 'primary'}
            onClick={executeAction}
            disabled={mutating}
          >
            {mutating ? 'Processing...' : actionLabel[confirmAction?.action ?? '']}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
