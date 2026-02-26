import { SiteShell } from "../components/site-shell";
import { routeContent } from "../components/route-content";

export default function HomePage() {
  return (
    <SiteShell
      heading={routeContent.home.heading}
      description={routeContent.home.description}
      actionText={routeContent.home.actionText}
    />
  );
}

