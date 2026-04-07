import { useState } from 'react';
import { useQuery } from '@apollo/client/react';
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
  TextField,
  InputAdornment,
  CircularProgress,
  Alert,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  Tabs,
  Tab,
  List,
  ListItem,
  ListItemText,
  Divider,
} from '@mui/material';
import {
  Search as SearchIcon,
  Visibility as ViewIcon,
} from '@mui/icons-material';
import { GET_USERS, GET_USER_ADDRESSES, GET_ORDERS_BY_CUSTOMER, GET_PAYMENTS_BY_CUSTOMER } from '../graphql/customers';

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phone: string;
  role: string;
  createdAt: string;
}

interface Address {
  id: number;
  line1: string;
  line2: string | null;
  city: string;
  county: string;
  postCode: string;
  country: string;
  isDefault: boolean;
}

interface Order {
  orderId: string;
  status: string;
  totalAmount: number;
  createdAt: string;
}

interface PaymentRecord {
  id: number;
  orderId: string;
  amount: number;
  currency: string;
  status: string;
  createdAt: string;
}

function CustomerDetail({ userId, onClose }: { userId: string; onClose: () => void }) {
  const [tab, setTab] = useState(0);

  const { data: addrData, loading: addrLoading } = useQuery(GET_USER_ADDRESSES, {
    variables: { userId },
  });
  const { data: ordersData, loading: ordersLoading } = useQuery(GET_ORDERS_BY_CUSTOMER, {
    variables: { customerId: userId },
  });
  const { data: paymentsData, loading: paymentsLoading } = useQuery(GET_PAYMENTS_BY_CUSTOMER, {
    variables: { customerId: userId },
  });

  const addresses: Address[] = addrData?.userAddresses ?? [];
  const orders: Order[] = ordersData?.ordersByCustomer ?? [];
  const payments: PaymentRecord[] = paymentsData?.paymentsByCustomer ?? [];

  return (
    <Dialog open onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>Customer Detail</DialogTitle>
      <DialogContent>
        <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 2 }}>
          <Tab label={`Addresses (${addresses.length})`} />
          <Tab label={`Orders (${orders.length})`} />
          <Tab label={`Payments (${payments.length})`} />
        </Tabs>

        {tab === 0 && (
          addrLoading ? <CircularProgress /> : addresses.length === 0 ? (
            <Typography color="text.secondary">No addresses on file.</Typography>
          ) : (
            <List disablePadding>
              {addresses.map((addr) => (
                <Box key={addr.id}>
                  <ListItem>
                    <ListItemText
                      primary={
                        <>
                          {addr.line1}{addr.line2 ? `, ${addr.line2}` : ''}
                          {addr.isDefault && (
                            <Chip label="Default" size="small" color="primary" sx={{ ml: 1 }} />
                          )}
                        </>
                      }
                      secondary={`${addr.city}, ${addr.county ? addr.county + ', ' : ''}${addr.postCode}, ${addr.country}`}
                    />
                  </ListItem>
                  <Divider />
                </Box>
              ))}
            </List>
          )
        )}

        {tab === 1 && (
          ordersLoading ? <CircularProgress /> : orders.length === 0 ? (
            <Typography color="text.secondary">No orders found.</Typography>
          ) : (
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Order ID</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell align="right">Total</TableCell>
                    <TableCell>Date</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {orders.map((o) => (
                    <TableRow key={o.orderId} hover>
                      <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                        {o.orderId.slice(0, 8)}...
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={o.status}
                          size="small"
                          color={
                            o.status === 'Completed' ? 'success' :
                            o.status === 'Cancelled' ? 'error' : 'default'
                          }
                        />
                      </TableCell>
                      <TableCell align="right">${o.totalAmount.toFixed(2)}</TableCell>
                      <TableCell>{new Date(o.createdAt).toLocaleDateString()}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )
        )}

        {tab === 2 && (
          paymentsLoading ? <CircularProgress /> : payments.length === 0 ? (
            <Typography color="text.secondary">No payments found.</Typography>
          ) : (
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>ID</TableCell>
                    <TableCell>Order</TableCell>
                    <TableCell align="right">Amount</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Date</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {payments.map((p) => (
                    <TableRow key={p.id} hover>
                      <TableCell>{p.id}</TableCell>
                      <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                        {p.orderId.slice(0, 8)}...
                      </TableCell>
                      <TableCell align="right">
                        {p.amount.toFixed(2)} {p.currency.toUpperCase()}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={p.status}
                          size="small"
                          color={p.status === 'Succeeded' ? 'success' : 'default'}
                        />
                      </TableCell>
                      <TableCell>{new Date(p.createdAt).toLocaleDateString()}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )
        )}
      </DialogContent>
    </Dialog>
  );
}

export default function CustomersPage() {
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [selectedUser, setSelectedUser] = useState<string | null>(null);

  const { data, loading, error } = useQuery(GET_USERS, {
    variables: { page: page + 1, pageSize, search: search || null },
    fetchPolicy: 'cache-and-network',
  });

  const users: User[] = data?.users?.items ?? [];
  const totalCount: number = data?.users?.totalCount ?? 0;

  const handleSearch = () => {
    setSearch(searchInput);
    setPage(0);
  };

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 3 }}>Customers</Typography>

      <TextField
        placeholder="Search by name or email..."
        size="small"
        value={searchInput}
        onChange={(e) => setSearchInput(e.target.value)}
        onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
        sx={{ mb: 2, width: 350 }}
        slotProps={{
          input: {
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
          },
        }}
      />

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          Failed to load customers: {error.message}
        </Alert>
      )}

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Phone</TableCell>
              <TableCell>Role</TableCell>
              <TableCell>Joined</TableCell>
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
            ) : users.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                  <Typography color="text.secondary">No customers found</Typography>
                </TableCell>
              </TableRow>
            ) : (
              users.map((user) => (
                <TableRow key={user.id} hover>
                  <TableCell>
                    <Typography variant="body2" fontWeight={500}>
                      {user.firstName} {user.lastName}
                    </Typography>
                  </TableCell>
                  <TableCell>{user.email}</TableCell>
                  <TableCell>{user.phone || '—'}</TableCell>
                  <TableCell>
                    <Chip
                      label={user.role}
                      size="small"
                      color={user.role === 'Admin' ? 'primary' : 'default'}
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell>{new Date(user.createdAt).toLocaleDateString()}</TableCell>
                  <TableCell align="right">
                    <IconButton
                      size="small"
                      onClick={() => setSelectedUser(user.id)}
                      title="View details"
                    >
                      <ViewIcon fontSize="small" />
                    </IconButton>
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

      {selectedUser && (
        <CustomerDetail userId={selectedUser} onClose={() => setSelectedUser(null)} />
      )}
    </Box>
  );
}
