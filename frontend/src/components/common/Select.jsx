import { forwardRef } from 'react';
import clsx from 'clsx';

const Select = forwardRef(({
  label,
  options = [],
  error = '',
  required = false,
  placeholder = 'Select an option',
  className = '',
  ...props
}, ref) => {
  return (
    <div className="space-y-1.5 w-full">
      {label && (
        <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
          {label}
          {required && <span className="text-red-500 ml-1">*</span>}
        </label>
      )}

      <div className="relative">
        <select
          ref={ref}
          className={clsx(
            'block w-full h-11 rounded-lg border text-sm transition-all appearance-none',
            'bg-slate-50 dark:bg-slate-800',
            'text-slate-900 dark:text-white',
            'focus:outline-none focus:ring-2 focus:ring-primary/10',
            'px-3 pr-10',
            error
              ? 'border-red-300 dark:border-red-700 focus:border-red-500'
              : 'border-slate-200 dark:border-slate-700 focus:border-primary',
            className
          )}
          {...props}
        >
          {placeholder && (
            <option value="">{placeholder}</option>
          )}
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>

        <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3">
          <span className="material-symbols-outlined text-slate-400 text-[20px]">
            expand_more
          </span>
        </div>
      </div>

      {error && (
        <p className="text-xs text-red-600 dark:text-red-400 mt-1">{error}</p>
      )}
    </div>
  );
});

Select.displayName = 'Select';

export default Select;
