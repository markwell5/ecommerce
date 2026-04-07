import { useMemo } from 'react';
import { useQuery } from '@apollo/client/react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  CircularProgress,
  Alert,
} from '@mui/material';
import {
  ShoppingCart as OrdersIcon,
  AttachMoney as RevenueIcon,
  Inventory as ProductsIcon,
  Warning as AlertIcon,
} from '@mui/icons-material';
import { GET_DASHBOARD_ORDERS, GET_DASHBOARD_PRODUCTS } from '../graphql/dashboard';

function MetricCard({ title, value, icon, color }: {
  title: string;
  value: string;
  icon: React.ReactNode;
  color: string;
}) {
  return (
    <Paper sx={{ p: 3, display: 'flex', alignItems: 'center', gap: 2 }}>
      <Box
        sx={{
          bgcolor: color,
          color: 'white',
          borderRadius: 2,
          p: 1.5,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        {icon}
      </Box>
      <Box>
        <Typography variant="body2" color="text.secondary">
          {title}
        </Typography>
        <Typography variant="h5" fontWeight={600}>
          {value}
        </Typography>
      </Box>
    </Paper>
  );
}

export default function DashboardPage() {
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

  const metrics = useMemo(() => {
    const orders = ordersData?.orders?.items ?? [];
    const products = productsData?.products?.items ?? [];

    const totalRevenue = orders.reduce((sum: number, o: { totalAmount: number }) => sum + o.totalAmount, 0);
    const orderCount = orders.length;
    const avgOrderValue = orderCount > 0 ? totalRevenue / orderCount : 0;
    const lowStockCount = products.filter(
      (p: { stockLevel?: { availableQuantity: number } }) =>
        (p.stockLevel?.availableQuantity ?? 0) <= 10
    ).length;

    return { totalRevenue, orderCount, avgOrderValue, lowStockCount };
  }, [ordersData, productsData]);

  if (loading && !ordersData && !productsData) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 3 }}>Dashboard</Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          Failed to load dashboard data: {error.message}
        </Alert>
      )}

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <MetricCard
            title="Total Orders"
            value={metrics.orderCount.toLocaleString()}
            icon={<OrdersIcon />}
            color="#1976d2"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <MetricCard
            title="Total Revenue"
            value={`$${metrics.totalRevenue.toFixed(2)}`}
            icon={<RevenueIcon />}
            color="#2e7d32"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <MetricCard
            title="Avg Order Value"
            value={`$${metrics.avgOrderValue.toFixed(2)}`}
            icon={<ProductsIcon />}
            color="#ed6c02"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <MetricCard
            title="Low Stock Items"
            value={metrics.lowStockCount.toString()}
            icon={<AlertIcon />}
            color={metrics.lowStockCount > 0 ? '#d32f2f' : '#9e9e9e'}
          />
        </Grid>
      </Grid>
    </Box>
  );
}
