# Loca Database Schema — COMPLETE (PostgreSQL 16 + PostGIS)

## identity schema
```sql
CREATE TABLE identity.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE,
    phone VARCHAR(20),
    display_name VARCHAR(100) NOT NULL,
    bio VARCHAR(500),
    avatar_url VARCHAR(500),
    thumbnail_url VARCHAR(500),
    date_of_birth DATE NOT NULL,
    gender VARCHAR(20) NOT NULL CHECK (gender IN ('male','female','other','prefer_not_to_say')),
    auth_provider VARCHAR(20) NOT NULL,
    auth_provider_id VARCHAR(255),
    is_onboarded BOOLEAN DEFAULT FALSE,
    is_anonymous_default BOOLEAN DEFAULT FALSE,
    is_premium BOOLEAN DEFAULT FALSE,
    premium_expires_at TIMESTAMPTZ,
    is_active BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    deleted_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE identity.user_interests (
    user_id UUID REFERENCES identity.users(id) ON DELETE CASCADE,
    interest VARCHAR(50) NOT NULL,
    PRIMARY KEY (user_id, interest)
);

CREATE TABLE identity.user_purposes (
    user_id UUID REFERENCES identity.users(id) ON DELETE CASCADE,
    purpose VARCHAR(50) NOT NULL,
    PRIMARY KEY (user_id, purpose)
);

CREATE TABLE identity.user_vibe_preferences (
    user_id UUID REFERENCES identity.users(id) ON DELETE CASCADE,
    vibe VARCHAR(50) NOT NULL,
    weight DECIMAL(3,2) DEFAULT 1.0,
    PRIMARY KEY (user_id, vibe)
);

CREATE TABLE identity.refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES identity.users(id) ON DELETE CASCADE,
    token_hash VARCHAR(512) NOT NULL,
    device_fingerprint VARCHAR(255),
    expires_at TIMESTAMPTZ NOT NULL,
    revoked_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_refresh_tokens_user ON identity.refresh_tokens(user_id) WHERE revoked_at IS NULL;
```

## venue schema
```sql
CREATE TABLE venue.venues (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    description TEXT,
    address VARCHAR(500) NOT NULL,
    category VARCHAR(50) NOT NULL,
    location GEOGRAPHY(POINT, 4326) NOT NULL,
    geofence_radius_meters INT DEFAULT 150,
    cover_photo_url VARCHAR(500),
    photo_urls TEXT[],
    google_rating DECIMAL(2,1),
    phone VARCHAR(20),
    website VARCHAR(500),
    working_hours JSONB,
    is_active BOOLEAN DEFAULT TRUE,
    owner_user_id UUID,
    subscription_plan VARCHAR(20) DEFAULT 'basic',
    qr_secret_key VARCHAR(64) NOT NULL DEFAULT encode(gen_random_bytes(32), 'hex'),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_venues_location ON venue.venues USING GIST(location);
CREATE INDEX idx_venues_category ON venue.venues(category);
CREATE INDEX idx_venues_owner ON venue.venues(owner_user_id);

CREATE TABLE venue.checkins (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    venue_id UUID REFERENCES venue.venues(id),
    qr_payload_hash VARCHAR(128) NOT NULL,
    check_in_at TIMESTAMPTZ DEFAULT NOW(),
    check_out_at TIMESTAMPTZ,
    check_out_reason VARCHAR(20),
    is_anonymous BOOLEAN DEFAULT FALSE,
    device_fingerprint VARCHAR(255),
    lat DECIMAL(10,7),
    lng DECIMAL(10,7)
);
CREATE INDEX idx_checkins_venue_active ON venue.checkins(venue_id, check_out_at) WHERE check_out_at IS NULL;
CREATE INDEX idx_checkins_user ON venue.checkins(user_id, check_in_at DESC);
CREATE INDEX idx_checkins_rate_limit ON venue.checkins(user_id, venue_id, check_in_at DESC);

CREATE TABLE venue.events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    venue_id UUID REFERENCES venue.venues(id) ON DELETE CASCADE,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    image_url VARCHAR(500),
    starts_at TIMESTAMPTZ NOT NULL,
    ends_at TIMESTAMPTZ,
    is_promoted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_events_venue ON venue.events(venue_id, starts_at DESC);
```

## social schema
```sql
CREATE TABLE social.messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    room_id VARCHAR(100) NOT NULL,
    sender_id UUID NOT NULL,
    message_type VARCHAR(20) NOT NULL,
    content TEXT,
    media_url VARCHAR(500),
    reply_to_id UUID REFERENCES social.messages(id),
    metadata JSONB,
    is_pinned BOOLEAN DEFAULT FALSE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_messages_room ON social.messages(room_id, created_at DESC);

CREATE TABLE social.message_reactions (
    message_id UUID REFERENCES social.messages(id) ON DELETE CASCADE,
    user_id UUID NOT NULL,
    emoji VARCHAR(10) NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    PRIMARY KEY (message_id, user_id, emoji)
);

CREATE TABLE social.posts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    venue_id UUID NOT NULL,
    content TEXT,
    media_urls TEXT[],
    media_type VARCHAR(20) DEFAULT 'photo',
    like_count INT DEFAULT 0,
    comment_count INT DEFAULT 0,
    is_memory BOOLEAN DEFAULT TRUE,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_posts_venue ON social.posts(venue_id, created_at DESC);
CREATE INDEX idx_posts_user ON social.posts(user_id, created_at DESC);

CREATE TABLE social.likes (
    post_id UUID REFERENCES social.posts(id) ON DELETE CASCADE,
    user_id UUID NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    PRIMARY KEY (post_id, user_id)
);

CREATE TABLE social.comments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id UUID REFERENCES social.posts(id) ON DELETE CASCADE,
    user_id UUID NOT NULL,
    content VARCHAR(500) NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_comments_post ON social.comments(post_id, created_at DESC);

CREATE TABLE social.match_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sender_id UUID NOT NULL,
    receiver_id UUID NOT NULL,
    venue_id UUID NOT NULL,
    intro_message VARCHAR(200),
    status VARCHAR(20) DEFAULT 'pending',
    responded_at TIMESTAMPTZ,
    expires_at TIMESTAMPTZ DEFAULT NOW() + INTERVAL '48 hours',
    created_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_matches_receiver ON social.match_requests(receiver_id, status);
CREATE INDEX idx_matches_daily ON social.match_requests(sender_id, created_at) WHERE status = 'pending';

CREATE TABLE social.conversations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    match_request_id UUID REFERENCES social.match_requests(id),
    participant_1_id UUID NOT NULL,
    participant_2_id UUID NOT NULL,
    last_message_at TIMESTAMPTZ,
    last_message_preview VARCHAR(100),
    unread_count_1 INT DEFAULT 0,
    unread_count_2 INT DEFAULT 0,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE social.reports (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    reporter_id UUID NOT NULL,
    reported_id UUID NOT NULL,
    reason VARCHAR(50) NOT NULL,
    description TEXT,
    status VARCHAR(20) DEFAULT 'pending',
    reviewed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE social.blocks (
    blocker_id UUID NOT NULL,
    blocked_id UUID NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    PRIMARY KEY (blocker_id, blocked_id)
);

CREATE TABLE social.vibe_bombs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sender_id UUID NOT NULL,
    receiver_id UUID NOT NULL,
    venue_id UUID NOT NULL,
    coin_cost INT DEFAULT 100,
    hints JSONB DEFAULT '[]',
    guesses_used INT DEFAULT 0,
    max_guesses INT DEFAULT 3,
    is_revealed BOOLEAN DEFAULT FALSE,
    reveal_conversation_id UUID,
    expires_at TIMESTAMPTZ DEFAULT NOW() + INTERVAL '24 hours',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE social.time_capsules (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    venue_id UUID NOT NULL,
    message TEXT,
    media_urls TEXT[],
    unlock_at TIMESTAMPTZ NOT NULL,
    is_unlocked BOOLEAN DEFAULT FALSE,
    unlocked_at TIMESTAMPTZ,
    is_shared BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_capsules_unlock ON social.time_capsules(unlock_at) WHERE is_unlocked = FALSE;
```

## game schema
```sql
CREATE TABLE game.game_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    venue_id UUID NOT NULL,
    game_type VARCHAR(30) NOT NULL,
    host_user_id UUID NOT NULL,
    status VARCHAR(20) DEFAULT 'lobby',
    min_players INT NOT NULL,
    max_players INT NOT NULL,
    settings JSONB,
    state JSONB,
    current_phase VARCHAR(50),
    phase_deadline TIMESTAMPTZ,
    started_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_games_venue_active ON game.game_sessions(venue_id, status) WHERE status IN ('lobby','in_progress');

CREATE TABLE game.game_players (
    session_id UUID REFERENCES game.game_sessions(id) ON DELETE CASCADE,
    user_id UUID NOT NULL,
    role VARCHAR(50),
    score INT DEFAULT 0,
    is_alive BOOLEAN DEFAULT TRUE,
    is_connected BOOLEAN DEFAULT TRUE,
    disconnected_at TIMESTAMPTZ,
    joined_at TIMESTAMPTZ DEFAULT NOW(),
    PRIMARY KEY (session_id, user_id)
);

CREATE TABLE game.truth_or_dare_questions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    type VARCHAR(10) NOT NULL CHECK (type IN ('truth','dare')),
    category VARCHAR(30) NOT NULL,
    content_az TEXT NOT NULL,
    content_en TEXT,
    difficulty VARCHAR(10) DEFAULT 'medium',
    is_active BOOLEAN DEFAULT TRUE
);

CREATE TABLE game.quiz_questions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category VARCHAR(50) NOT NULL,
    question_az TEXT NOT NULL,
    answers_az JSONB NOT NULL,
    correct_index INT NOT NULL,
    difficulty VARCHAR(10) DEFAULT 'medium',
    is_active BOOLEAN DEFAULT TRUE
);

CREATE TABLE game.would_you_rather (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    option_a_az TEXT NOT NULL,
    option_b_az TEXT NOT NULL,
    category VARCHAR(30) DEFAULT 'general',
    is_active BOOLEAN DEFAULT TRUE
);

CREATE TABLE game.leaderboard_entries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    venue_id UUID,
    game_type VARCHAR(30) NOT NULL,
    score INT NOT NULL,
    week_start DATE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_leaderboard_weekly ON game.leaderboard_entries(game_type, week_start DESC, score DESC);
```

## economy schema
```sql
CREATE TABLE economy.wallets (
    user_id UUID PRIMARY KEY,
    coin_balance INT DEFAULT 0 CHECK (coin_balance >= 0),
    total_purchased INT DEFAULT 0,
    total_spent INT DEFAULT 0,
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE economy.coin_packages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL,
    name_az VARCHAR(50),
    price_azn DECIMAL(10,2) NOT NULL,
    coins INT NOT NULL,
    bonus_coins INT DEFAULT 0,
    ios_product_id VARCHAR(100),
    android_product_id VARCHAR(100),
    sort_order INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE
);

CREATE TABLE economy.iap_receipts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    platform VARCHAR(10) NOT NULL,
    product_id VARCHAR(100) NOT NULL,
    receipt_data TEXT NOT NULL,
    transaction_id VARCHAR(255) UNIQUE NOT NULL,
    is_valid BOOLEAN DEFAULT TRUE,
    coins_credited INT NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_iap_transaction ON economy.iap_receipts(transaction_id);

CREATE TABLE economy.transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    type VARCHAR(20) NOT NULL,
    amount INT NOT NULL,
    balance_after INT NOT NULL,
    reference_type VARCHAR(30),
    reference_id UUID,
    description VARCHAR(200),
    created_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_transactions_user ON economy.transactions(user_id, created_at DESC);

CREATE TABLE economy.gift_catalog (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    name_az VARCHAR(100),
    tier VARCHAR(20) NOT NULL,
    coin_price INT NOT NULL,
    animation_url VARCHAR(500),
    icon_url VARCHAR(500),
    venue_id UUID,
    is_active BOOLEAN DEFAULT TRUE,
    sort_order INT DEFAULT 0
);
```

## notification schema
```sql
CREATE TABLE notification.device_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    token VARCHAR(500) NOT NULL,
    platform VARCHAR(10) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(user_id, token)
);

CREATE TABLE notification.notification_log (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    type VARCHAR(50) NOT NULL,
    title VARCHAR(200),
    body VARCHAR(500),
    data JSONB,
    is_sent BOOLEAN DEFAULT FALSE,
    sent_at TIMESTAMPTZ,
    is_read BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_notif_user ON notification.notification_log(user_id, created_at DESC);
```

## venue schema (Phase 3 additions)
```sql
CREATE TABLE venue.song_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    venue_id UUID REFERENCES venue.venues(id),
    user_id UUID NOT NULL,
    spotify_track_id VARCHAR(100) NOT NULL,
    track_name VARCHAR(300),
    artist_name VARCHAR(300),
    coin_cost INT DEFAULT 20,
    upvotes INT DEFAULT 0,
    downvotes INT DEFAULT 0,
    status VARCHAR(20) DEFAULT 'pending',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE venue.chain_parties (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    venue_chain UUID[] NOT NULL,
    reward_coins INT DEFAULT 500,
    time_limit_hours INT DEFAULT 4,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE venue.chain_participants (
    chain_id UUID REFERENCES venue.chain_parties(id),
    user_id UUID NOT NULL,
    venues_completed UUID[] DEFAULT '{}',
    is_complete BOOLEAN DEFAULT FALSE,
    completed_at TIMESTAMPTZ,
    joined_at TIMESTAMPTZ DEFAULT NOW(),
    PRIMARY KEY (chain_id, user_id)
);
```
