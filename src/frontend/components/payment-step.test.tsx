import { describe, expect, it, jest } from "@jest/globals";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { PaymentStep } from "./payment-step";

describe("PaymentStep", () => {
  it("renders payment handoff state for a created order", async () => {
    const user = userEvent.setup();
    const createPayment = jest.fn().mockResolvedValue({
      provider: "Razorpay",
      amountInr: 190,
      currency: "INR",
      razorpayOrderId: "order_test_frontend",
      razorpayKeyId: "rzp_test_placeholder"
    });

    render(
      <PaymentStep
        order={{
          orderId: "order-123",
          reference: "ORD-123456",
          status: "PendingPayment",
          subtotalInr: 190,
          totalInr: 190
        }}
        createPayment={createPayment}
      />
    );

    expect(screen.getByText("Order reference ORD-123456 is ready for payment.")).toBeInTheDocument();
    expect(screen.getByText("Backend verification remains the source of truth for payment status.")).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Continue to Payment" }));

    await waitFor(() => {
      expect(createPayment).toHaveBeenCalledWith("order-123");
    });

    expect(screen.getByText("Razorpay order order_test_frontend created.")).toBeInTheDocument();
    expect(
      screen.getByText("Awaiting verified backend confirmation before marking this order paid.")
    ).toBeInTheDocument();
  });
});
