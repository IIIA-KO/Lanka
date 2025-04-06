﻿// <auto-generated />
using System;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Lanka.Modules.Campaigns.Infrastructure.Migrations
{
    [DbContext(typeof(CampaignsDbContext))]
    partial class CampaignsDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("campaigns")
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Lanka.Common.Infrastructure.Inbox.InboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("jsonb")
                        .HasColumnName("content");

                    b.Property<string>("Error")
                        .HasColumnType("text")
                        .HasColumnName("error");

                    b.Property<DateTime>("OccurredOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("occurred_on_utc");

                    b.Property<DateTime?>("ProcessedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_on_utc");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_inbox_messages");

                    b.ToTable("inbox_messages", "campaigns");
                });

            modelBuilder.Entity("Lanka.Common.Infrastructure.Inbox.InboxMessageConsumer", b =>
                {
                    b.Property<Guid>("InboxMessageId")
                        .HasColumnType("uuid")
                        .HasColumnName("inbox_message_id");

                    b.Property<string>("Name")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("name");

                    b.HasKey("InboxMessageId", "Name")
                        .HasName("pk_inbox_message_consumers");

                    b.ToTable("inbox_message_consumers", "campaigns");
                });

            modelBuilder.Entity("Lanka.Common.Infrastructure.Outbox.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("jsonb")
                        .HasColumnName("content");

                    b.Property<string>("Error")
                        .HasColumnType("text")
                        .HasColumnName("error");

                    b.Property<DateTime>("OccurredOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("occurred_on_utc");

                    b.Property<DateTime?>("ProcessedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_on_utc");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_outbox_messages");

                    b.ToTable("outbox_messages", "campaigns");
                });

            modelBuilder.Entity("Lanka.Common.Infrastructure.Outbox.OutboxMessageConsumer", b =>
                {
                    b.Property<Guid>("OutboxMessageId")
                        .HasColumnType("uuid")
                        .HasColumnName("outbox_message_id");

                    b.Property<string>("Name")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("name");

                    b.HasKey("OutboxMessageId", "Name")
                        .HasName("pk_outbox_message_consumers");

                    b.ToTable("outbox_message_consumers", "campaigns");
                });

            modelBuilder.Entity("Lanka.Modules.Campaigns.Domain.Bloggers.Blogger", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateOnly>("BirthDate")
                        .HasColumnType("date")
                        .HasColumnName("birth_date");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("first_name");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("last_name");

                    b.HasKey("Id")
                        .HasName("pk_bloggers");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasDatabaseName("ix_bloggers_email");

                    b.ToTable("bloggers", "campaigns");
                });

            modelBuilder.Entity("Lanka.Modules.Campaigns.Domain.Campaigns.Campaign", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset?>("CancelledOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("cancelled_on_utc");

                    b.Property<Guid>("ClientId")
                        .HasColumnType("uuid")
                        .HasColumnName("client_id");

                    b.Property<DateTimeOffset?>("CompletedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("completed_on_utc");

                    b.Property<DateTimeOffset?>("ConfirmedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("confirmed_on_utc");

                    b.Property<Guid>("CreatorId")
                        .HasColumnType("uuid")
                        .HasColumnName("creator_id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)")
                        .HasColumnName("description");

                    b.Property<DateTimeOffset?>("DoneOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("done_on_utc");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("name");

                    b.Property<Guid>("OfferId")
                        .HasColumnType("uuid")
                        .HasColumnName("offer_id");

                    b.Property<DateTimeOffset>("PendedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("pended_on_utc");

                    b.Property<DateTimeOffset?>("RejectedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("rejected_on_utc");

                    b.Property<DateTimeOffset>("ScheduledOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("scheduled_on_utc");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.HasKey("Id")
                        .HasName("pk_campaigns");

                    b.HasIndex("ClientId")
                        .HasDatabaseName("ix_campaigns_client_id");

                    b.HasIndex("CreatorId")
                        .HasDatabaseName("ix_campaigns_creator_id");

                    b.HasIndex("OfferId")
                        .HasDatabaseName("ix_campaigns_offer_id");

                    b.ToTable("campaigns", "campaigns");
                });

            modelBuilder.Entity("Lanka.Modules.Campaigns.Domain.Offers.Offer", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("description");

                    b.Property<DateTimeOffset?>("LastCooperatedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_cooperated_on_utc");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.Property<Guid>("PactId")
                        .HasColumnType("uuid")
                        .HasColumnName("pact_id");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id")
                        .HasName("pk_offers");

                    b.HasIndex("PactId")
                        .HasDatabaseName("ix_offers_pact_id");

                    b.ToTable("offers", "campaigns");
                });

            modelBuilder.Entity("Lanka.Modules.Campaigns.Domain.Pacts.Pact", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("BloggerId")
                        .HasColumnType("uuid")
                        .HasColumnName("blogger_id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<DateTimeOffset>("LastUpdatedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_updated_on_utc");

                    b.HasKey("Id")
                        .HasName("pk_pacts");

                    b.HasIndex("BloggerId")
                        .IsUnique()
                        .HasDatabaseName("ix_pacts_blogger_id");

                    b.ToTable("pacts", "campaigns");
                });

            modelBuilder.Entity("Lanka.Modules.Campaigns.Domain.Campaigns.Campaign", b =>
                {
                    b.HasOne("Lanka.Modules.Campaigns.Domain.Bloggers.Blogger", "Client")
                        .WithMany()
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("fk_campaigns_bloggers_client_id");

                    b.HasOne("Lanka.Modules.Campaigns.Domain.Bloggers.Blogger", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("fk_campaigns_bloggers_creator_id");

                    b.HasOne("Lanka.Modules.Campaigns.Domain.Offers.Offer", "Offer")
                        .WithMany()
                        .HasForeignKey("OfferId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_campaigns_offers_offer_id");

                    b.OwnsOne("Lanka.Common.Contracts.Monies.Money", "Price", b1 =>
                        {
                            b1.Property<Guid>("CampaignId")
                                .HasColumnType("uuid")
                                .HasColumnName("id");

                            b1.Property<string>("Currency")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("price_currency");

                            b1.HasKey("CampaignId");

                            b1.ToTable("campaigns", "campaigns");

                            b1.WithOwner()
                                .HasForeignKey("CampaignId")
                                .HasConstraintName("fk_campaigns_campaigns_id");
                        });

                    b.Navigation("Client");

                    b.Navigation("Creator");

                    b.Navigation("Offer");

                    b.Navigation("Price")
                        .IsRequired();
                });

            modelBuilder.Entity("Lanka.Modules.Campaigns.Domain.Offers.Offer", b =>
                {
                    b.HasOne("Lanka.Modules.Campaigns.Domain.Pacts.Pact", "Pact")
                        .WithMany("Offers")
                        .HasForeignKey("PactId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_offers_pacts_pact_id");

                    b.OwnsOne("Lanka.Common.Contracts.Monies.Money", "Price", b1 =>
                        {
                            b1.Property<Guid>("OfferId")
                                .HasColumnType("uuid")
                                .HasColumnName("id");

                            b1.Property<string>("Currency")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("price_currency");

                            b1.HasKey("OfferId");

                            b1.ToTable("offers", "campaigns");

                            b1.WithOwner()
                                .HasForeignKey("OfferId")
                                .HasConstraintName("fk_offers_offers_id");
                        });

                    b.Navigation("Pact");

                    b.Navigation("Price")
                        .IsRequired();
                });

            modelBuilder.Entity("Lanka.Modules.Campaigns.Domain.Pacts.Pact", b =>
                {
                    b.HasOne("Lanka.Modules.Campaigns.Domain.Bloggers.Blogger", "Blogger")
                        .WithOne("Pact")
                        .HasForeignKey("Lanka.Modules.Campaigns.Domain.Pacts.Pact", "BloggerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("fk_pacts_bloggers_blogger_id");

                    b.Navigation("Blogger");
                });

            modelBuilder.Entity("Lanka.Modules.Campaigns.Domain.Bloggers.Blogger", b =>
                {
                    b.Navigation("Pact");
                });

            modelBuilder.Entity("Lanka.Modules.Campaigns.Domain.Pacts.Pact", b =>
                {
                    b.Navigation("Offers");
                });
#pragma warning restore 612, 618
        }
    }
}
