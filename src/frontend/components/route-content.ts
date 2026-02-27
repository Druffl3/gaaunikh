export type RouteContent = {
  heading: string;
  description: string;
  actionText: string;
};

export const routeContent = {
  home: {
    heading: "Pure Spice Craft From Gaaunikh Kitchens",
    description:
      "Freshly ground masalas with rooted family recipes, packed in small batches for dependable aroma in every meal.",
    actionText: "Discover the Brand"
  },
  shop: {
    heading: "Catalog Collection",
    description:
      "Browse single-origin spices and house blends with transparent pricing by pack size.",
    actionText: "Browse Collection"
  },
  productDetail: {
    heading: "Product Details",
    description: "Review full flavor notes and choose the right weight variant for your kitchen.",
    actionText: "Back to Catalog"
  },
  trackOrder: {
    heading: "Track Orders in One Place",
    description:
      "Order tracking is being connected now. Soon you can follow every package from packing station to doorstep.",
    actionText: "Track Coming Soon"
  },
  contact: {
    heading: "Reach the Gaaunikh Team",
    description:
      "Need bulk orders or partnership details? Contact us and our operations team will respond with procurement options.",
    actionText: "Send an Enquiry"
  }
} satisfies Record<string, RouteContent>;

