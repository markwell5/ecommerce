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
  TrendingUp as AvgIcon,
  PersonAdd as CustomersIcon,
} from '@mui/icons-material';
import { GET_SALES_OVERVIEW } from '../graphql/dashboard';

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
        }}
      >
        {icon}
      </Box>
      <Box>
        <Typography variant="body2" color="text.secondary">{title}</Typography>
        <Typography variant="h5" fontWeight={600}>{value}</Typography>
      </Box>
    </Paper>
  );
}

export default function DashboardPage() {
  const { data, loading, error } = useQuery(GET_SALES_OVERVIEW, {
    fetchPolicy: 'cache-and-network',
  });

  const overview = data?.salesOverview;

  if (loading && !data) {
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
            value={overview?.orderCount?.toLocaleString() ?? '0'}
            icon={<OrdersIcon />}
            color="#1976d2"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <MetricCard
            title="Revenue"
            value={`$${(overview?.totalRevenue ?? 0).toFixed(2)}`}
            icon={<RevenueIcon />}
            color="#2e7d32"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <MetricCard
            title="Avg Order Value"
            value={`$${(overview?.avgOrderValue ?? 0).toFixed(2)}`}
            icon={<AvgIcon />}
            color="#ed6c02"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <MetricCard
            title="New Customers"
            value={overview?.newCustomerCount?.toString() ?? '0'}
            icon={<CustomersIcon />}
            color="#9c27b0"
          />
        </Grid>
      </Grid>
    </Box>
  );
}
