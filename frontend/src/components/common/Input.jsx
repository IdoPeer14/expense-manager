import { forwardRef } from 'react';
import clsx from 'clsx';

const Input = forwardRef(({
  label,
  type = 'text',
  placeholder = '',
  error = '',
  icon = null,
  required = false,
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
        {icon && (
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <span className="material-symbols-outlined text-slate-400 dark:text-slate-500 text-[20px]">
              {icon}
            </span>
          </div>
        )}

        <input
          ref={ref}
          type={type}
          placeholder={placeholder}
          className={clsx(
            'block w-full h-11 rounded-lg border text-sm transition-all',
            'bg-slate-50 dark:bg-slate-800',
            'text-slate-900 dark:text-white',
            'placeholder:text-slate-400 dark:placeholder:text-slate-500',
            'focus:outline-none focus:ring-2 focus:ring-primary/10',
            icon ? 'pl-10 pr-3' : 'px-3',
            error
              ? 'border-red-300 dark:border-red-700 focus:border-red-500'
              : 'border-slate-200 dark:border-slate-700 focus:border-primary',
            className
          )}
          {...props}
        />
      </div>

      {error && (
        <p className="text-xs text-red-600 dark:text-red-400 mt-1">{error}</p>
      )}
    </div>
  );
});

Input.displayName = 'Input';

export default Input;
