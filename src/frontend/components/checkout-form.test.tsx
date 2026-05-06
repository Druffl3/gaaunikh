import { describe, expect, it, jest } from "@jest/globals";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { CheckoutForm } from "./checkout-form";
import type { CartLine } from "../lib/cart";

const testLines: CartLine[] = [
  {
    id: "kashmiri-chili-powder::100g",
    productSlug: "kashmiri-chili-powder",
    productName: "Kashmiri Chili Powder",
    weightLabel: "100g",
    unitPriceInr: 95,
    quantity: 2
  }
];

describe("CheckoutForm", () => {
  it("renders checkout fields and submits successfully", async () => {
    const user = userEvent.setup();
    const submitCheckout = jest.fn().mockResolvedValue({
      reference: "ORD-123456",
      status: "PendingPayment",
      subtotalInr: 190,
      totalInr: 190
    });

    render(<CheckoutForm lines={testLines} submitCheckout={submitCheckout} />);

    await user.type(screen.getByLabelText("Full Name"), "Asha Raman");
    await user.type(screen.getByLabelText("Email"), "asha@example.com");
    await user.type(screen.getByLabelText("Phone"), "+919999999999");
    await user.type(screen.getByLabelText("Address Line 1"), "12 Spice Market Road");
    await user.type(screen.getByLabelText("Address Line 2"), "Floor 2");
    await user.type(screen.getByLabelText("City"), "Bengaluru");
    await user.type(screen.getByLabelText("State"), "Karnataka");
    await user.type(screen.getByLabelText("Postal Code"), "560001");
    await user.type(screen.getByLabelText("Country Code"), "IN");

    await user.click(screen.getByRole("button", { name: "Place Order" }));

    await waitFor(() => {
      expect(submitCheckout).toHaveBeenCalledWith({
        customerName: "Asha Raman",
        customerEmail: "asha@example.com",
        customerPhone: "+919999999999",
        shippingAddress: {
          line1: "12 Spice Market Road",
          line2: "Floor 2",
          city: "Bengaluru",
          state: "Karnataka",
          postalCode: "560001",
          countryCode: "IN"
        },
        lines: [
          {
            productSlug: "kashmiri-chili-powder",
            weightLabel: "100g",
            unitPriceInr: 95,
            quantity: 2
          }
        ]
      });
    });

    expect(screen.getByText("Order reference ORD-123456 created successfully.")).toBeInTheDocument();
  });
});
