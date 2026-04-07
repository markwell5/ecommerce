import { gql } from '@apollo/client';

export const GET_SALES_OVERVIEW = gql`
  query GetSalesOverview($from: String, $to: String) {
    salesOverview(from: $from, to: $to) {
      totalRevenue
      orderCount
      avgOrderValue
      cancelledCount
      returnedCount
      newCustomerCount
    }
  }
`;

export const GET_ORDER_STATUS_BREAKDOWN = gql`
  query GetOrderStatusBreakdown($from: String, $to: String) {
    orderStatusBreakdown(from: $from, to: $to) {
      status
      count
    }
  }
`;

export const GET_DAILY_REVENUE = gql`
  query GetDailyRevenue($from: String, $to: String) {
    dailyRevenue(from: $from, to: $to) {
      date
      revenue
      orderCount
    }
  }
`;

export const GET_DASHBOARD_PRODUCTS = gql`
  query GetDashboardProducts($page: Int!, $pageSize: Int!) {
    products(page: $page, pageSize: $pageSize) {
      items {
        id
        name
        category
        price
        stockLevel {
          availableQuantity
        }
      }
      totalCount
    }
  }
`;
