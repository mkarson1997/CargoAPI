# Domain Rules

This document separates confirmed business behavior from decisions that are still open. It exists so code changes do not silently invent pricing policy.

## Confirmed rules

### Order desi must be positive

`orderDesi <= 0` is rejected and no order is persisted.

### Matching ranges use the cheapest eligible configuration

When multiple active carrier configurations contain the requested desi, the configuration with the lowest `CarrierCost` is selected.

Example:

```text
Order desi: 5
Carrier A range: 1–10, price 40
Carrier B range: 1–10, price 32

Selected: Carrier B, 32 TRY
```

### Documented above-range fallback

When no range matches and the selected closest configuration has a maximum below the order desi, the current documented formula is:

```text
finalPrice = carrierPrice + (carrierPlusDesiCost × (orderDesi - carrierMaxDesi))
```

Example:

```text
Order desi: 13
Max desi: 10
Base price: 32 TRY
Extra-desi price: 4 TRY

Final: 32 + (4 × 3) = 44 TRY
```

### No available active configuration

If no applicable/closest active configuration can be returned, order creation fails and nothing is persisted.

## Open domain decision

The current implementation finds the closest `CarrierMaxDesi` using absolute distance and also uses an absolute difference in fallback pricing.

That leaves two cases that need explicit product/business policy:

1. the order is **below every configured range**,
2. the order falls in a **gap between ranges**.

The repository intentionally tracks this as an open issue instead of changing the behavior from engineering intuition alone.

## Required tests before rule changes

Any resolution should add tests for:

- exact lower boundary,
- exact upper boundary,
- overlapping ranges,
- above-all-ranges order,
- below-all-ranges order,
- gap between two ranges,
- equal-price tie behavior if relevant,
- inactive carrier configurations,
- no active configuration.

## Principle

Business policy should be explicit in documentation and tests before implementation is changed.
