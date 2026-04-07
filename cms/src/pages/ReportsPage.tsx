import { useMemo } from 'react';
import { useQuery } from '@apollo/client/react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  CircularProgress,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
} from '@mui/material';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Legend,
  LineChart,
  Line,
  CartesianGrid,
} from 'recharts';
import { GET_DASHBOARD_ORDERS, GET_DASHBOARD_PRODUCTS } from '../graphql/dashboard';

const STATUS_COLORS: Record<string, string> = {
  Placed: '#2196f3',
  Processing: '#ff9800',
  Shipped: '#ff9800',
  Delivered: '#4caf50',
  Completed: '#4caf50',
  Cancelled: '#f44336',
  Returned: '#f44336',
};

interface OrderItem {
  orderId: string;
  status: string;
  totalAmount: number;
  createdAt: string;
}

interface ProductItem {
  id: number;
  name: string;
  category: string;
  price: number;
  stockLevel?: { availableQuantity: number };
}

export default function ReportsPage() {
  const { data: ordersData, loading: ordersLoading, error: ordersError } = useQuery(
    GET_DASHBOARD_ORDERS,
    { variables: { page: 1, pageSize: 1000 }, fetchPolicy: 'cache-and-network' }
  );

  const { data: productsData, loading: productsLoading, error: productsError } = useQuery(
    GET_DASHBOARD_PRODUCTS,
    { variables: { page: 1, pageSize: 1000 }, fetchPolicy: 'cache-and-network' }
  );

  const loading = ordersLoading || productsLoading;
  const error = ordersError || productsError;

  const orders: OrderItem[] = ordersData?.orders?.items ?? [];
  const products: ProductItem[] = productsData?.products?.items ?? [];

  // Order status breakdown
  const statusData = useMemo(() => {
    const counts: Record<string, number> = {};
    orders.forEach((o) => {
      counts[o.status] = (counts[o.status] || 0) + 1;
    });
    return Object.entries(counts).map(([name, value]) => ({ name, value }));
  }, [orders]);

  // Revenue over time (grouped by day)
  const revenueOverTime = useMemo(() => {
    const daily: Record<string, number> = {};
    orders.forEach((o) => {
      const date = new Date(o.createdAt).toLocaleDateString();
      daily[date] = (daily[date] || 0) + o.totalAmount;
    });
    return Object.entries(daily)
      .map(([date, revenue]) => ({ date, revenue: Math.round(revenue * 100) / 100 }))
      .sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime())
      .slice(-30); // last 30 days
  }, [orders]);

  // Top products by revenue (estimate from category/price)
  const topProducts = useMemo(() => {
    return [...products]
      .sort((a, b) => b.price - a.price)
      .slice(0, 10);
  }, [products]);

  // Low stock alerts
  const lowStockProducts = useMemo(() => {
    return products
      .filter((p) => (p.stockLevel?.availableQuantity ?? 0) <= 10)
      .sort((a, b) => (a.stockLevel?.availableQuantity ?? 0) - (b.stockLevel?.availableQuantity ?? 0));
  }, [products]);

  if (loading && !ordersData && !productsData) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 3 }}>Reports</Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          Failed to load report data: {error.message}
        </Alert>
      )}

      <Grid container spacing={3}>
        {/* Revenue over time */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>Revenue Over Time</Typography>
            {revenueOverTime.length === 0 ? (
              <Typography color="text.secondary">No revenue data available.</Typography>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={revenueOverTime}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" fontSize={12} />
                  <YAxis fontSize={12} />
                  <Tooltip formatter={(value: number) => [`$${value.toFixed(2)}`, 'Revenue']} />
                  <Line type="monotone" dataKey="revenue" stroke="#1976d2" strokeWidth={2} dot={false} />
                </LineChart>
              </ResponsiveContainer>
            )}
          </Paper>
        </Grid>

        {/* Order status breakdown */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>Order Status</Typography>
            {statusData.length === 0 ? (
              <Typography color="text.secondary">No order data available.</Typography>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={statusData}
                    dataKey="value"
                    nameKey="name"
                    cx="50%"
                    cy="50%"
                    outerRadius={90}
                    label={({ name, value }) => `${name}: ${value}`}
                  >
                    {statusData.map((entry) => (
                      <Cell key={entry.name} fill={STATUS_COLORS[entry.name] || '#9e9e9e'} />
                    ))}
                  </Pie>
                  <Legend />
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            )}
          </Paper>
        </Grid>

        {/* Top products */}
        <Grid size={{ xs: 12, md: 6 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>Top Products by Price</Typography>
            {topProducts.length === 0 ? (
              <Typography color="text.secondary">No product data available.</Typography>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={topProducts} layout="vertical">
                  <XAxis type="number" fontSize={12} />
                  <YAxis dataKey="name" type="category" width={120} fontSize={11} />
                  <Tooltip formatter={(value: number) => [`$${value.toFixed(2)}`, 'Price']} />
                  <Bar dataKey="price" fill="#2e7d32" />
                </BarChart>
              </ResponsiveContainer>
            )}
          </Paper>
        </Grid>

        {/* Low stock alerts */}
        <Grid size={{ xs: 12, md: 6 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Low Stock Alerts
              {lowStockProducts.length > 0 && (
                <Chip
                  label={lowStockProducts.length}
                  size="small"
                  color="error"
                  sx={{ ml: 1 }}
                />
              )}
            </Typography>
            {lowStockProducts.length === 0 ? (
              <Typography color="text.secondary">All products are well stocked.</Typography>
            ) : (
              <TableContainer sx={{ maxHeight: 260 }}>
                <Table size="small" stickyHeader>
                  <TableHead>
                    <TableRow>
                      <TableCell>Product</TableCell>
                      <TableCell>Category</TableCell>
                      <TableCell align="right">Stock</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {lowStockProducts.map((p) => (
                      <TableRow key={p.id}>
                        <TableCell>{p.name}</TableCell>
                        <TableCell>
                          <Chip label={p.category} size="small" />
                        </TableCell>
                        <TableCell align="right">
                          <Chip
                            label={p.stockLevel?.availableQuantity ?? 0}
                            size="small"
                            color={
                              (p.stockLevel?.availableQuantity ?? 0) === 0 ? 'error' : 'warning'
                            }
                          />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
}
