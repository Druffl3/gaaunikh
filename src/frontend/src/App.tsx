import { NavLink, Route, Routes } from "react-router-dom";

type RouteInfo = {
  path: string;
  label: string;
  heading: string;
  description: string;
  actionText: string;
};

const routes: RouteInfo[] = [
  {
    path: "/",
    label: "Home",
    heading: "Pure Spice Craft From Gaaunikh Kitchens",
    description:
      "Freshly ground masalas with rooted family recipes, packed in small batches for dependable aroma in every meal.",
    actionText: "Discover the Brand"
  },
  {
    path: "/shop",
    label: "Shop",
    heading: "Shop Launching Next",
    description:
      "Our product catalog is preparing for release. Expect turmeric, chili, coriander, garam masala, and signature blends.",
    actionText: "Get Notified"
  },
  {
    path: "/track-order",
    label: "Track Order",
    heading: "Track Orders in One Place",
    description:
      "Order tracking is being connected now. Soon you can follow every package from packing station to doorstep.",
    actionText: "Track Coming Soon"
  },
  {
    path: "/contact",
    label: "Contact",
    heading: "Reach the Gaaunikh Team",
    description:
      "Need bulk orders or partnership details? Contact us and our operations team will respond with procurement options.",
    actionText: "Send an Enquiry"
  }
];

function RouteSection({ route }: { route: RouteInfo }) {
  return (
    <main className="hero-wrap">
      <section className="hero-card">
        <p className="eyebrow">Gaaunikh Masala</p>
        <h1>{route.heading}</h1>
        <p>{route.description}</p>
        <button type="button">{route.actionText}</button>
      </section>
      <aside className="highlight-card">
        <h2>Batch Promise</h2>
        <p>
          Every pack is sourced, roasted, and milled for flavor retention and everyday consistency.
        </p>
        <ul>
          <li>Single-origin spice selection</li>
          <li>Fresh grind cycles every week</li>
          <li>Sealed for aroma protection</li>
        </ul>
      </aside>
    </main>
  );
}

export default function App() {
  return (
    <div className="app-shell">
      <header className="topbar">
        <a className="brand" href="/">
          Gaaunikh
          <span>Masala</span>
        </a>
        <nav aria-label="Primary">
          {routes.map((route) => (
            <NavLink
              key={route.path}
              className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}
              end={route.path === "/"}
              to={route.path}
            >
              {route.label}
            </NavLink>
          ))}
        </nav>
      </header>

      <Routes>
        {routes.map((route) => (
          <Route key={route.path} element={<RouteSection route={route} />} path={route.path} />
        ))}
        <Route element={<RouteSection route={routes[0]} />} path="*" />
      </Routes>

      <footer>
        <p>Flavor built for homes, restaurants, and wholesale partners.</p>
      </footer>
    </div>
  );
}
