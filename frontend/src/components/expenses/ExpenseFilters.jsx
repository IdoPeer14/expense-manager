import { useState } from 'react';
import Input from '../common/Input';
import Select from '../common/Select';
import Button from '../common/Button';
import { EXPENSE_CATEGORIES } from '../../utils/constants';

const DATE_RANGE_OPTIONS = [
  { value: 'last30', label: 'Last 30 Days' },
  { value: 'thisMonth', label: 'This Month' },
  { value: 'lastQuarter', label: 'Last Quarter' },
  { value: 'custom', label: 'Custom Range' },
];

const ExpenseFilters = ({ onApply, onClear, loading }) => {
  const [filters, setFilters] = useState({
    dateRange: 'last30',
    startDate: '',
    endDate: '',
    minAmount: '',
    maxAmount: '',
    category: '',
    businessName: '',
  });

  const handleChange = (field, value) => {
    setFilters((prev) => ({ ...prev, [field]: value }));
  };

  const handleApply = () => {
    // Calculate date range based on selection
    const now = new Date();
    let startDate = filters.startDate;
    let endDate = filters.endDate;

    if (filters.dateRange === 'last30') {
      const thirtyDaysAgo = new Date(now);
      thirtyDaysAgo.setDate(now.getDate() - 30);
      startDate = thirtyDaysAgo.toISOString().split('T')[0];
      endDate = now.toISOString().split('T')[0];
    } else if (filters.dateRange === 'thisMonth') {
      startDate = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split('T')[0];
      endDate = now.toISOString().split('T')[0];
    } else if (filters.dateRange === 'lastQuarter') {
      const threeMonthsAgo = new Date(now);
      threeMonthsAgo.setMonth(now.getMonth() - 3);
      startDate = threeMonthsAgo.toISOString().split('T')[0];
      endDate = now.toISOString().split('T')[0];
    }

    const params = {
      startDate: startDate || undefined,
      endDate: endDate || undefined,
      minAmount: filters.minAmount ? parseFloat(filters.minAmount) : undefined,
      maxAmount: filters.maxAmount ? parseFloat(filters.maxAmount) : undefined,
      category: filters.category || undefined,
      businessName: filters.businessName || undefined,
    };

    onApply(params);
  };

  const handleClear = () => {
    setFilters({
      dateRange: 'last30',
      startDate: '',
      endDate: '',
      minAmount: '',
      maxAmount: '',
      category: '',
      businessName: '',
    });
    onClear();
  };

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <Select
          label="Date Range"
          options={DATE_RANGE_OPTIONS}
          value={filters.dateRange}
          onChange={(e) => handleChange('dateRange', e.target.value)}
        />

        {filters.dateRange === 'custom' && (
          <>
            <Input
              label="Start Date"
              type="date"
              value={filters.startDate}
              onChange={(e) => handleChange('startDate', e.target.value)}
            />
            <Input
              label="End Date"
              type="date"
              value={filters.endDate}
              onChange={(e) => handleChange('endDate', e.target.value)}
            />
          </>
        )}

        <Input
          label="Min Amount"
          type="number"
          step="0.01"
          placeholder="0.00"
          value={filters.minAmount}
          onChange={(e) => handleChange('minAmount', e.target.value)}
        />

        <Input
          label="Max Amount"
          type="number"
          step="0.01"
          placeholder="0.00"
          value={filters.maxAmount}
          onChange={(e) => handleChange('maxAmount', e.target.value)}
        />

        <Select
          label="Category"
          options={[{ value: '', label: 'All Categories' }, ...EXPENSE_CATEGORIES]}
          value={filters.category}
          onChange={(e) => handleChange('category', e.target.value)}
        />

        <Input
          label="Business Name"
          placeholder="Search by vendor..."
          value={filters.businessName}
          onChange={(e) => handleChange('businessName', e.target.value)}
        />
      </div>

      <div className="flex gap-3">
        <Button
          type="button"
          variant="primary"
          onClick={handleApply}
          loading={loading}
        >
          Apply Filters
        </Button>
        <Button type="button" variant="secondary" onClick={handleClear}>
          Clear
        </Button>
      </div>
    </div>
  );
};

export default ExpenseFilters;
