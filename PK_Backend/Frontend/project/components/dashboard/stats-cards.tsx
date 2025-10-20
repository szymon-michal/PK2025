import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Users, MessageCircle, Code, TrendingUp } from 'lucide-react';

interface StatsCardsProps {
  stats: {
    totalFriends: number;
    pendingRequests: number;
    unreadMessages: number;
    repositories: number;
    matchScore: number;
  };
}

export default function StatsCards({ stats }: StatsCardsProps) {
  const cards = [
    {
      title: 'Friends',
      value: stats.totalFriends,
      description: `${stats.pendingRequests} pending requests`,
      icon: Users,
      color: 'text-blue-600',
      bgColor: 'bg-blue-100'
    },
    {
      title: 'Messages',
      value: stats.unreadMessages,
      description: 'Unread messages',
      icon: MessageCircle,
      color: 'text-green-600',
      bgColor: 'bg-green-100'
    },
    {
      title: 'Repositories',
      value: stats.repositories,
      description: 'Code projects',
      icon: Code,
      color: 'text-purple-600',
      bgColor: 'bg-purple-100'
    },
    {
      title: 'Match Score',
      value: `${Math.round(stats.matchScore * 100)}%`,
      description: 'Average compatibility',
      icon: TrendingUp,
      color: 'text-orange-600',
      bgColor: 'bg-orange-100'
    }
  ];

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      {cards.map((card, index) => (
        <Card key={index} className="relative overflow-hidden">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center justify-between">
              {card.title}
              <div className={`p-2 rounded-lg ${card.bgColor}`}>
                <card.icon className={`h-4 w-4 ${card.color}`} />
              </div>
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{card.value}</div>
            <p className="text-xs text-muted-foreground mt-1">{card.description}</p>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}